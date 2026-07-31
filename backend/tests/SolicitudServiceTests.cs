using Aplicacion.DTOs;
using Aplicacion.Excepciones;
using Aplicacion.Servicios;
using Dominio;
using Dominio.Excepciones;
using Dominio.Servicios;
using Tests.Fakes;

namespace Tests;

public class SolicitudServiceTests
{
    private readonly Guid _tenantA = Guid.NewGuid();
    private readonly Guid _tenantB = Guid.NewGuid();
    private readonly Guid _solicitanteA = Guid.NewGuid();
    private readonly Guid _solicitanteB = Guid.NewGuid();
    private readonly Guid _agente = Guid.NewGuid();
    private readonly Guid _admin = Guid.NewGuid();
    private readonly Guid _categoriaIncidente;
    private readonly FakeSolicitudRepository _solicitudes = new();
    private readonly FakeCategoriaRepository _categorias = new();
    private readonly FakeUsuarioRepository _usuarios = new();
    private readonly SolicitudService _service;

    public SolicitudServiceTests()
    {
        _categoriaIncidente = Guid.NewGuid();
        _categorias.Data.Add(new Categoria
        {
            Id = _categoriaIncidente,
            TenantId = _tenantA,
            Nombre = "Incidente",
            SlaHoras = 8,
            Activo = true,
        });
        _categorias.Data.Add(new Categoria
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantB,
            Nombre = "Consulta",
            SlaHoras = 24,
            Activo = true,
        });

        _usuarios.Data.Add(new Usuario
        {
            Id = _agente,
            TenantId = _tenantA,
            Nombre = "Agente 1",
            Email = "agente1@norte.test",
            Rol = Rol.Agente,
            Activo = true,
        });
        _usuarios.Data.Add(new Usuario
        {
            Id = _admin,
            TenantId = _tenantA,
            Nombre = "Admin",
            Email = "admin@norte.test",
            Rol = Rol.Admin,
            Activo = true,
        });

        _service = new SolicitudService(
            _solicitudes,
            _categorias,
            _usuarios,
            new StateMachineService(),
            new SlaCalculator(),
            new CodigoFormateador(),
            new PermissionService());
    }

    [Fact]
    public async Task Crear_GeneraCodigoCorrelativoPorAnioYCalculaSla()
    {
        var request = new SolicitudCreateRequest
        {
            Titulo = "No puedo acceder al portal",
            Descripcion = "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login.",
            CategoriaId = _categoriaIncidente,
            Prioridad = Prioridad.Critica,
        };

        var resultado = await _service.CrearAsync(_tenantA, _solicitanteA, request);

        var anio = DateTime.UtcNow.Year;
        Assert.Equal($"SOL-{anio}-00001", resultado.Codigo);
        Assert.Equal(EstadoSolicitud.Nueva, resultado.Estado);
        Assert.Equal(_tenantA, resultado.TenantId);
        Assert.Equal(_solicitanteA, resultado.SolicitanteId);
        Assert.Equal(resultado.FechaCreacion.AddHours(4), resultado.FechaLimiteSla);
    }

    [Fact]
    public async Task Crear_ConCategoriaDeOtroTenant_LanzaRecursoNoEncontrado()
    {
        var categoriaSur = _categorias.Data.Single(c => c.TenantId == _tenantB);
        var request = new SolicitudCreateRequest
        {
            Titulo = "Título válido",
            Descripcion = "Descripción válida de más de diez caracteres.",
            CategoriaId = categoriaSur.Id,
            Prioridad = Prioridad.Media,
        };

        var ex = await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => _service.CrearAsync(_tenantA, _solicitanteA, request));

        Assert.Equal("RECURSO_NO_ENCONTRADO", ex.Codigo);
    }

    [Fact]
    public async Task Crear_ConCamposInvalidos_LanzaValidacion()
    {
        var request = new SolicitudCreateRequest
        {
            Titulo = "abc",
            Descripcion = "corta",
            CategoriaId = _categoriaIncidente,
            Prioridad = Prioridad.Media,
        };

        var ex = await Assert.ThrowsAsync<ValidacionException>(
            () => _service.CrearAsync(_tenantA, _solicitanteA, request));

        Assert.Equal("VALIDACION", ex.Codigo);
        Assert.True(ex.Errores.ContainsKey("titulo"));
        Assert.True(ex.Errores.ContainsKey("descripcion"));
    }

    [Fact]
    public async Task EjecutarTransicion_CrossTenant_LanzaRecursoNoEncontrado()
    {
        var solicitudB = CrearSolicitud(_tenantB, _solicitanteB, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(solicitudB);

        var request = new SolicitudTransicionRequest { Accion = "asignar", AgenteId = _agente };

        var ex = await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => _service.EjecutarTransicionAsync(solicitudB.Id, _tenantA, _admin, Rol.Admin, request));

        Assert.Equal("RECURSO_NO_ENCONTRADO", ex.Codigo);
    }

    [Fact]
    public async Task EjecutarTransicion_SolicitanteCancelaPropia_LanzaPermisoDenegado()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudTransicionRequest
        {
            Accion = "cancelar",
            Motivo = "Duplicada de SOL-2026-00012.",
        };

        var ex = await Assert.ThrowsAsync<PermisoDenegadoException>(
            () => _service.EjecutarTransicionAsync(propia.Id, _tenantA, _solicitanteA, Rol.Solicitante, request));

        Assert.Equal("OPERACION_NO_PERMITIDA", ex.Codigo);
    }

    [Fact]
    public async Task EjecutarTransicion_TransicionInvalida_LanzaTransicionInvalida()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudTransicionRequest
        {
            Accion = "resolver",
            Motivo = "Se restableció la contraseña del usuario y se validó el acceso.",
        };

        var ex = await Assert.ThrowsAsync<TransicionInvalidaException>(
            () => _service.EjecutarTransicionAsync(propia.Id, _tenantA, _admin, Rol.Admin, request));

        Assert.Equal("TRANSICION_INVALIDA", ex.Codigo);
    }

    [Fact]
    public async Task EjecutarTransicion_AsignarConAgenteInexistente_LanzaAgenteInvalido()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudTransicionRequest
        {
            Accion = "asignar",
            AgenteId = Guid.NewGuid(),
        };

        var ex = await Assert.ThrowsAsync<AgenteInvalidoException>(
            () => _service.EjecutarTransicionAsync(propia.Id, _tenantA, _admin, Rol.Admin, request));

        Assert.Equal("AGENTE_INVALIDO", ex.Codigo);
    }

    [Fact]
    public async Task EjecutarTransicion_AsignarConAgenteDeOtroTenant_LanzaAgenteInvalido()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var agenteSur = new Usuario
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantB,
            Nombre = "Agente Sur",
            Email = "agente@sur.test",
            Rol = Rol.Agente,
            Activo = true,
        };
        _usuarios.Data.Add(agenteSur);

        var request = new SolicitudTransicionRequest
        {
            Accion = "asignar",
            AgenteId = agenteSur.Id,
        };

        var ex = await Assert.ThrowsAsync<AgenteInvalidoException>(
            () => _service.EjecutarTransicionAsync(propia.Id, _tenantA, _admin, Rol.Admin, request));

        Assert.Equal("AGENTE_INVALIDO", ex.Codigo);
    }

    [Fact]
    public async Task EjecutarTransicion_AsignarValido_AsignaAgente()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudTransicionRequest { Accion = "asignar", AgenteId = _agente };

        var resultado = await _service.EjecutarTransicionAsync(
            propia.Id, _tenantA, _admin, Rol.Admin, request);

        Assert.Equal(EstadoSolicitud.Asignada, resultado.Estado);
        Assert.Equal(_agente, resultado.AgenteId);
    }

    [Fact]
    public async Task EjecutarTransicion_ResolverConMotivoCorto_LanzaMotivoRequerido()
    {
        var enProceso = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.EnProceso);
        enProceso.AgenteId = _agente;
        _solicitudes.Data.Add(enProceso);

        var request = new SolicitudTransicionRequest
        {
            Accion = "resolver",
            Motivo = "motivo corto",
        };

        var ex = await Assert.ThrowsAsync<MotivoRequeridoException>(
            () => _service.EjecutarTransicionAsync(enProceso.Id, _tenantA, _admin, Rol.Admin, request));

        Assert.Equal("MOTIVO_REQUERIDO", ex.Codigo);
    }

    [Fact]
    public async Task EjecutarTransicion_ResolverConMotivoVeinteCaracteres_Resuelve()
    {
        var enProceso = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.EnProceso);
        enProceso.AgenteId = _agente;
        _solicitudes.Data.Add(enProceso);

        var request = new SolicitudTransicionRequest
        {
            Accion = "resolver",
            Motivo = "Se restableció la contraseña del usuario y se validó el acceso.",
        };

        var resultado = await _service.EjecutarTransicionAsync(
            enProceso.Id, _tenantA, _admin, Rol.Admin, request);

        Assert.Equal(EstadoSolicitud.Resuelta, resultado.Estado);
        Assert.Equal(request.Motivo, resultado.MotivoResolucion);
        Assert.NotNull(resultado.FechaResolucion);
    }

    [Fact]
    public async Task EjecutarTransicion_CancelarConMotivoCorto_LanzaMotivoRequerido()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudTransicionRequest
        {
            Accion = "cancelar",
            Motivo = "nuevechar",
        };

        var ex = await Assert.ThrowsAsync<MotivoRequeridoException>(
            () => _service.EjecutarTransicionAsync(propia.Id, _tenantA, _admin, Rol.Admin, request));

        Assert.Equal("MOTIVO_REQUERIDO", ex.Codigo);
    }

    [Fact]
    public async Task EjecutarTransicion_CancelarConMotivoDiezCaracteres_Cancela()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudTransicionRequest
        {
            Accion = "cancelar",
            Motivo = "Duplicada de SOL-2026-00012.",
        };

        var resultado = await _service.EjecutarTransicionAsync(
            propia.Id, _tenantA, _admin, Rol.Admin, request);

        Assert.Equal(EstadoSolicitud.Cancelada, resultado.Estado);
        Assert.Equal(request.Motivo, resultado.MotivoCancelacion);
    }

    [Fact]
    public async Task Editar_CambiaPrioridad_RecalculaSla()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudEditRequest
        {
            Titulo = "Título editado válido",
            Descripcion = "Descripción editada de más de diez caracteres.",
            CategoriaId = _categoriaIncidente,
            Prioridad = Prioridad.Critica,
        };

        var resultado = await _service.EditarAsync(
            propia.Id, _tenantA, _admin, Rol.Admin, request);

        Assert.Equal(Prioridad.Critica, resultado.Prioridad);
        Assert.Equal(resultado.FechaCreacion.AddHours(4), resultado.FechaLimiteSla);
        Assert.Equal(resultado.FechaCreacion, propia.FechaCreacion);
    }

    [Fact]
    public async Task Editar_SolicitudResuelta_NoRecalculaSla()
    {
        var resuelta = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Resuelta);
        var slaOriginal = resuelta.FechaLimiteSla;
        _solicitudes.Data.Add(resuelta);

        var request = new SolicitudEditRequest
        {
            Titulo = "Título editado válido",
            Descripcion = "Descripción editada de más de diez caracteres.",
            CategoriaId = _categoriaIncidente,
            Prioridad = Prioridad.Critica,
        };

        var resultado = await _service.EditarAsync(
            resuelta.Id, _tenantA, _admin, Rol.Admin, request);

        Assert.Equal(slaOriginal, resultado.FechaLimiteSla);
    }

    [Fact]
    public async Task Editar_SolicitanteAjena_LanzaPermisoDenegado()
    {
        var ajena = CrearSolicitud(_tenantA, _solicitanteB, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(ajena);

        var request = new SolicitudEditRequest
        {
            Titulo = "Título editado válido",
            Descripcion = "Descripción editada de más de diez caracteres.",
            CategoriaId = _categoriaIncidente,
            Prioridad = Prioridad.Media,
        };

        var ex = await Assert.ThrowsAsync<PermisoDenegadoException>(
            () => _service.EditarAsync(ajena.Id, _tenantA, _solicitanteA, Rol.Solicitante, request));

        Assert.Equal("OPERACION_NO_PERMITIDA", ex.Codigo);
    }

    [Fact]
    public async Task Editar_SolicitantePropiaFueraDeNueva_LanzaPermisoDenegado()
    {
        var propia = CrearSolicitud(_tenantA, _solicitanteA, EstadoSolicitud.Asignada);
        _solicitudes.Data.Add(propia);

        var request = new SolicitudEditRequest
        {
            Titulo = "Título editado válido",
            Descripcion = "Descripción editada de más de diez caracteres.",
            CategoriaId = _categoriaIncidente,
            Prioridad = Prioridad.Media,
        };

        var ex = await Assert.ThrowsAsync<PermisoDenegadoException>(
            () => _service.EditarAsync(propia.Id, _tenantA, _solicitanteA, Rol.Solicitante, request));

        Assert.Equal("OPERACION_NO_PERMITIDA", ex.Codigo);
    }

    [Fact]
    public async Task Obtener_CrossTenant_LanzaRecursoNoEncontrado()
    {
        var solicitudB = CrearSolicitud(_tenantB, _solicitanteB, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(solicitudB);

        var ex = await Assert.ThrowsAsync<RecursoNoEncontradoException>(
            () => _service.ObtenerAsync(solicitudB.Id, _tenantA, _admin, Rol.Admin));

        Assert.Equal("RECURSO_NO_ENCONTRADO", ex.Codigo);
    }

    [Fact]
    public async Task Obtener_SolicitanteAjenaMismoTenant_LanzaPermisoDenegado()
    {
        var ajena = CrearSolicitud(_tenantA, _solicitanteB, EstadoSolicitud.Nueva);
        _solicitudes.Data.Add(ajena);

        var ex = await Assert.ThrowsAsync<PermisoDenegadoException>(
            () => _service.ObtenerAsync(ajena.Id, _tenantA, _solicitanteA, Rol.Solicitante));

        Assert.Equal("OPERACION_NO_PERMITIDA", ex.Codigo);
    }

    [Fact]
    public async Task Listar_PageSizeMayor100_LanzaParametroInvalido()
    {
        var request = new SolicitudListaRequest { PageSize = 101 };

        var ex = await Assert.ThrowsAsync<ParametroInvalidoException>(
            () => _service.ListarAsync(_tenantA, _solicitanteA, Rol.Admin, request));

        Assert.Equal("PARAMETRO_INVALIDO", ex.Codigo);
    }

    [Fact]
    public async Task Listar_PageMenor1_LanzaParametroInvalido()
    {
        var request = new SolicitudListaRequest { Page = 0 };

        var ex = await Assert.ThrowsAsync<ParametroInvalidoException>(
            () => _service.ListarAsync(_tenantA, _solicitanteA, Rol.Admin, request));

        Assert.Equal("PARAMETRO_INVALIDO", ex.Codigo);
    }

    [Fact]
    public async Task Listar_Solicitante_FiltraSoloSusSolicitudes()
    {
        var request = new SolicitudListaRequest();

        await _service.ListarAsync(_tenantA, _solicitanteA, Rol.Solicitante, request);

        Assert.Equal(_solicitanteA, _solicitudes.UltimoFiltros!.SolicitanteId);
    }

    [Fact]
    public async Task Listar_Admin_NoFiltraPorSolicitante()
    {
        var request = new SolicitudListaRequest();

        await _service.ListarAsync(_tenantA, _solicitanteA, Rol.Admin, request);

        Assert.Null(_solicitudes.UltimoFiltros!.SolicitanteId);
    }

    private Solicitud CrearSolicitud(
        Guid tenantId, Guid solicitanteId, EstadoSolicitud estado)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = "SOL-2026-00001",
            Titulo = "Título válido",
            Descripcion = "Descripción válida de más de diez caracteres.",
            CategoriaId = _categoriaIncidente,
            Prioridad = Prioridad.Media,
            Estado = estado,
            SolicitanteId = solicitanteId,
            FechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc),
            FechaLimiteSla = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
        };
}
