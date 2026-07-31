using Aplicacion.DTOs;
using Aplicacion.Excepciones;
using Aplicacion.Interfaces;
using Dominio;
using Dominio.Excepciones;
using Dominio.Servicios;

namespace Aplicacion.Servicios;

public sealed class SolicitudService
{
    private readonly ISolicitudRepository _solicitudRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly StateMachineService _stateMachine;
    private readonly SlaCalculator _slaCalculator;
    private readonly CodigoFormateador _codigoFormateador;
    private readonly PermissionService _permisos;

    public SolicitudService(
        ISolicitudRepository solicitudRepository,
        ICategoriaRepository categoriaRepository,
        IUsuarioRepository usuarioRepository,
        StateMachineService stateMachine,
        SlaCalculator slaCalculator,
        CodigoFormateador codigoFormateador,
        PermissionService permisos)
    {
        _solicitudRepository = solicitudRepository;
        _categoriaRepository = categoriaRepository;
        _usuarioRepository = usuarioRepository;
        _stateMachine = stateMachine;
        _slaCalculator = slaCalculator;
        _codigoFormateador = codigoFormateador;
        _permisos = permisos;
    }

    public async Task<Solicitud> CrearAsync(Guid tenantId, Guid solicitanteId, SolicitudCreateRequest request)
    {
        var errores = ValidarCampos(request.Titulo, request.Descripcion);
        if (request.CategoriaId == Guid.Empty)
        {
            errores["categoriaId"] = ["La categoría es obligatoria."];
        }
        if (errores.Count > 0)
        {
            throw new ValidacionException(errores);
        }

        var categoria = await _categoriaRepository.GetByIdAsync(request.CategoriaId, tenantId);
        if (categoria is null)
        {
            throw new RecursoNoEncontradoException("La categoría no existe en su organización.");
        }

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            CategoriaId = categoria.Id,
            Prioridad = request.Prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = solicitanteId,
            FechaCreacion = DateTime.UtcNow,
        };

        var anio = solicitud.FechaCreacion.Year;
        var correlativo = await _solicitudRepository.ObtenerMaximoCorrelativoAsync(tenantId, anio) + 1;
        solicitud.Codigo = _codigoFormateador.Formatear(anio, correlativo);

        solicitud.FechaLimiteSla = _slaCalculator.Calcular(
            solicitud.FechaCreacion, categoria.SlaHoras, solicitud.Prioridad);

        await _solicitudRepository.AddAsync(solicitud);
        return solicitud;
    }

    public async Task<SolicitudPaginada> ListarAsync(
        Guid tenantId, Guid userId, Rol rol, SolicitudListaRequest request)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
        {
            throw new ParametroInvalidoException(
                "El parámetro 'page' debe ser mayor o igual a 1 y 'pageSize' debe estar entre 1 y 100.");
        }

        if (!SortValido(request.Sort))
        {
            throw new ParametroInvalidoException(
                "El parámetro 'sort' solo admite: fechaCreacion, -fechaCreacion, prioridad, -prioridad, codigo.");
        }

        var filtros = new SolicitudFiltros
        {
            TenantId = tenantId,
            Estado = request.Estado,
            Prioridad = request.Prioridad,
            CategoriaId = request.CategoriaId,
            AgenteId = request.AgenteId,
            Q = request.Q,
            Vencidas = request.Vencidas,
            Page = request.Page,
            PageSize = request.PageSize,
            Sort = request.Sort,
        };

        if (!_permisos.PuedoListar(rol))
        {
            filtros = filtros with { SolicitanteId = userId };
        }

        var resultado = await _solicitudRepository.ListarAsync(filtros);

        return new SolicitudPaginada
        {
            Items = resultado.Items,
            Page = request.Page,
            PageSize = request.PageSize,
            Total = resultado.Total,
            TotalPaginas = (int)Math.Ceiling(resultado.Total / (double)request.PageSize),
        };
    }

    public async Task<Solicitud> ObtenerAsync(Guid id, Guid tenantId, Guid userId, Rol rol)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(id, tenantId);
        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        if (!_permisos.PuedoVerDetalle(rol, solicitud.SolicitanteId, userId))
        {
            throw new PermisoDenegadoException("No tiene permiso para ver esta solicitud.");
        }

        return solicitud;
    }

    public async Task<Solicitud> EditarAsync(
        Guid id, Guid tenantId, Guid userId, Rol rol, SolicitudEditRequest request)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(id, tenantId);
        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        if (!_permisos.PuedoEditar(rol, solicitud.Estado, solicitud.SolicitanteId, userId))
        {
            throw new PermisoDenegadoException("No tiene permiso para editar esta solicitud.");
        }

        var errores = ValidarCampos(request.Titulo, request.Descripcion);
        if (request.CategoriaId == Guid.Empty)
        {
            errores["categoriaId"] = ["La categoría es obligatoria."];
        }
        if (errores.Count > 0)
        {
            throw new ValidacionException(errores);
        }

        var categoria = await _categoriaRepository.GetByIdAsync(request.CategoriaId, tenantId);
        if (categoria is null)
        {
            throw new RecursoNoEncontradoException("La categoría no existe en su organización.");
        }

        var recalcularSla = solicitud.CategoriaId != request.CategoriaId
                            || solicitud.Prioridad != request.Prioridad;
        var estadoFinal = solicitud.Estado is EstadoSolicitud.Resuelta
            or EstadoSolicitud.Cerrada
            or EstadoSolicitud.Cancelada;

        solicitud.Titulo = request.Titulo.Trim();
        solicitud.Descripcion = request.Descripcion.Trim();
        solicitud.CategoriaId = categoria.Id;
        solicitud.Prioridad = request.Prioridad;

        if (recalcularSla && !estadoFinal)
        {
            solicitud.FechaLimiteSla = _slaCalculator.Calcular(
                solicitud.FechaCreacion, categoria.SlaHoras, solicitud.Prioridad);
        }

        await _solicitudRepository.UpdateAsync(solicitud);
        return solicitud;
    }

    public async Task<Solicitud> EjecutarTransicionAsync(
        Guid id, Guid tenantId, Guid userId, Rol rol, SolicitudTransicionRequest request)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(id, tenantId);
        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        if (!_permisos.PuedoEjecutarTransicion(rol, request.Accion, solicitud.SolicitanteId, userId))
        {
            throw new PermisoDenegadoException("No tiene permiso para ejecutar esta acción.");
        }

        var nuevoEstado = _stateMachine.Transicionar(solicitud.Estado, request.Accion);

        if (request.Accion == "asignar")
        {
            var agente = request.AgenteId.HasValue
                ? await _usuarioRepository.GetByIdAsync(request.AgenteId.Value, tenantId)
                : null;

            if (agente is null
                || !agente.Activo
                || (agente.Rol != Rol.Agente && agente.Rol != Rol.Admin))
            {
                throw new AgenteInvalidoException();
            }

            solicitud.AgenteId = agente.Id;
        }
        else if (request.Accion == "resolver")
        {
            var motivo = request.Motivo?.Trim();
            if (string.IsNullOrEmpty(motivo) || motivo.Length < 20)
            {
                throw new MotivoRequeridoException(
                    "El motivo de resolución debe tener al menos 20 caracteres.");
            }

            solicitud.MotivoResolucion = motivo;
            solicitud.FechaResolucion = DateTime.UtcNow;
        }
        else if (request.Accion == "cancelar")
        {
            var motivo = request.Motivo?.Trim();
            if (string.IsNullOrEmpty(motivo) || motivo.Length < 10)
            {
                throw new MotivoRequeridoException(
                    "El motivo de cancelación debe tener al menos 10 caracteres.");
            }

            solicitud.MotivoCancelacion = motivo;
        }

        solicitud.Estado = nuevoEstado;
        await _solicitudRepository.UpdateAsync(solicitud);
        return solicitud;
    }

    private static bool SortValido(string? sort)
        => string.IsNullOrWhiteSpace(sort)
           || sort is "fechaCreacion" or "-fechaCreacion" or "prioridad" or "-prioridad" or "codigo";

    private static Dictionary<string, string[]> ValidarCampos(string? titulo, string? descripcion)
    {
        var errores = new Dictionary<string, string[]>();
        var t = titulo?.Trim() ?? string.Empty;
        var d = descripcion?.Trim() ?? string.Empty;

        if (t.Length < 5 || t.Length > 120)
        {
            errores["titulo"] = ["El título debe tener entre 5 y 120 caracteres."];
        }
        if (d.Length < 10 || d.Length > 4000)
        {
            errores["descripcion"] = ["La descripción debe tener entre 10 y 4000 caracteres."];
        }

        return errores;
    }
}
