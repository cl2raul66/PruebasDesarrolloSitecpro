using Dominio;
using Dominio.Servicios;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using EspecSolicitud = (Dominio.EstadoSolicitud Estado, Dominio.Prioridad Prioridad, int Categoria, System.Guid? Agente, System.Guid Solicitante, int OffsetHoras);

namespace Infraestructura.Data;

public sealed class DbSeeder(MesaSitecDbContext db, IConfiguration config)
{
    private static readonly Guid NorteId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SurId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid AdminNorteId = Guid.Parse("00000001-0000-0000-0000-000000000000");
    private static readonly Guid Agente1NorteId = Guid.Parse("00000002-0000-0000-0000-000000000000");
    private static readonly Guid Agente2NorteId = Guid.Parse("00000003-0000-0000-0000-000000000000");
    private static readonly Guid User1NorteId = Guid.Parse("00000004-0000-0000-0000-000000000000");
    private static readonly Guid User2NorteId = Guid.Parse("00000005-0000-0000-0000-000000000000");
    private static readonly Guid AdminSurId = Guid.Parse("10000001-0000-0000-0000-000000000000");
    private static readonly Guid User1SurId = Guid.Parse("10000002-0000-0000-0000-000000000000");

    private static readonly Guid IncidenteNorteId = Guid.Parse("20000001-0000-0000-0000-000000000000");
    private static readonly Guid RequerimientoNorteId = Guid.Parse("20000002-0000-0000-0000-000000000000");
    private static readonly Guid ConsultaNorteId = Guid.Parse("20000003-0000-0000-0000-000000000000");
    private static readonly Guid FallaCriticaNorteId = Guid.Parse("20000004-0000-0000-0000-000000000000");

    private static readonly Guid IncidenteSurId = Guid.Parse("30000001-0000-0000-0000-000000000000");
    private static readonly Guid RequerimientoSurId = Guid.Parse("30000002-0000-0000-0000-000000000000");
    private static readonly Guid ConsultaSurId = Guid.Parse("30000003-0000-0000-0000-000000000000");
    private static readonly Guid FallaCriticaSurId = Guid.Parse("30000004-0000-0000-0000-000000000000");

    private const string Password = "Sitec.2026";

    public async Task SeedAsync()
    {
        if (await db.Tenants.AnyAsync())
        {
            return;
        }

        var baseFecha = ObtenerFechaBase();
        var sla = new SlaCalculator();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password);

        var norte = new Tenant { Id = NorteId, Nombre = "Cooperativa Norte", Activo = true };
        var sur = new Tenant { Id = SurId, Nombre = "Bufete Sur", Activo = true };
        db.Tenants.AddRange(norte, sur);

        var usuarios = new[]
        {
            new Usuario { Id = AdminNorteId, TenantId = NorteId, Email = "admin@norte.test", PasswordHash = passwordHash, Nombre = "Admin Norte", Rol = Rol.Admin, Activo = true },
            new Usuario { Id = Agente1NorteId, TenantId = NorteId, Email = "agente1@norte.test", PasswordHash = passwordHash, Nombre = "Agente Uno Norte", Rol = Rol.Agente, Activo = true },
            new Usuario { Id = Agente2NorteId, TenantId = NorteId, Email = "agente2@norte.test", PasswordHash = passwordHash, Nombre = "Agente Dos Norte", Rol = Rol.Agente, Activo = true },
            new Usuario { Id = User1NorteId, TenantId = NorteId, Email = "user1@norte.test", PasswordHash = passwordHash, Nombre = "Solicitante Uno Norte", Rol = Rol.Solicitante, Activo = true },
            new Usuario { Id = User2NorteId, TenantId = NorteId, Email = "user2@norte.test", PasswordHash = passwordHash, Nombre = "Solicitante Dos Norte", Rol = Rol.Solicitante, Activo = true },
            new Usuario { Id = AdminSurId, TenantId = SurId, Email = "admin@sur.test", PasswordHash = passwordHash, Nombre = "Admin Sur", Rol = Rol.Admin, Activo = true },
            new Usuario { Id = User1SurId, TenantId = SurId, Email = "user1@sur.test", PasswordHash = passwordHash, Nombre = "Solicitante Sur", Rol = Rol.Solicitante, Activo = true },
        };
        db.Usuarios.AddRange(usuarios);

        var categoriasNorte = CrearCategorias(
            NorteId,
            (IncidenteNorteId, "Incidente", 8),
            (RequerimientoNorteId, "Requerimiento", 40),
            (ConsultaNorteId, "Consulta", 24),
            (FallaCriticaNorteId, "Falla crítica", 4));
        var categoriasSur = CrearCategorias(
            SurId,
            (IncidenteSurId, "Incidente", 8),
            (RequerimientoSurId, "Requerimiento", 40),
            (ConsultaSurId, "Consulta", 24),
            (FallaCriticaSurId, "Falla crítica", 4));
        db.Categorias.AddRange(categoriasNorte);
        db.Categorias.AddRange(categoriasSur);

        var codigoFormateador = new CodigoFormateador();

        var especificacionesNorte = EspecificacionesNorte();
        for (var i = 0; i < especificacionesNorte.Length; i++)
        {
            var codigo = codigoFormateador.Formatear(baseFecha.Year, i + 1);
            var s = especificacionesNorte[i];
            db.Solicitudes.Add(CrearSolicitud(
                NorteId, GuidPara(5000 + i), codigo, s, categoriasNorte[s.Categoria], baseFecha, sla));
        }

        var especificacionesSur = EspecificacionesSur();
        for (var i = 0; i < especificacionesSur.Length; i++)
        {
            var codigo = codigoFormateador.Formatear(baseFecha.Year, i + 1);
            var s = especificacionesSur[i];
            db.Solicitudes.Add(CrearSolicitud(
                SurId, GuidPara(8000 + i), codigo, s, categoriasSur[s.Categoria], baseFecha, sla));
        }

        await db.SaveChangesAsync();
    }

    private DateTime ObtenerFechaBase()
    {
        var valor = config["SEED_FECHA_BASE"];
        if (!string.IsNullOrWhiteSpace(valor)
            && DateTime.TryParse(valor, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var parse))
        {
            return parse;
        }

        return new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
    }

    private static List<Categoria> CrearCategorias(Guid tenantId, params (Guid Id, string Nombre, int SlaHoras)[] items)
        => items.Select(c => new Categoria
        {
            Id = c.Id,
            TenantId = tenantId,
            Nombre = c.Nombre,
            SlaHoras = c.SlaHoras,
            Activo = true,
        }).ToList();

    private static Solicitud CrearSolicitud(
        Guid tenantId,
        Guid id,
        string codigo,
        EspecSolicitud spec,
        Categoria categoria,
        DateTime baseFecha,
        SlaCalculator sla)
    {
        var creacion = baseFecha.AddHours(spec.OffsetHoras);

        var indice = int.Parse(codigo.Split('-')[2]) - 1;

        var solicitud = new Solicitud
        {
            Id = id,
            TenantId = tenantId,
            Codigo = codigo,
            Titulo = Titulos[indice % Titulos.Length],
            Descripcion = Descripciones[indice % Descripciones.Length],
            CategoriaId = categoria.Id,
            Prioridad = spec.Prioridad,
            Estado = spec.Estado,
            SolicitanteId = spec.Solicitante,
            AgenteId = spec.Agente,
            FechaCreacion = creacion,
            FechaLimiteSla = sla.Calcular(creacion, categoria.SlaHoras, spec.Prioridad),
        };

        if (spec.Estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
        {
            solicitud.FechaResolucion = creacion.AddHours(2);
            solicitud.MotivoResolucion = "Se restableció el acceso del usuario y se verificó que la solución quedó aplicada.";
        }

        if (spec.Estado is EstadoSolicitud.Cancelada)
        {
            solicitud.MotivoCancelacion = "La solicitud es duplicada de otra ya registrada.";
        }

        return solicitud;
    }

    private static Guid GuidPara(int seed)
        => Guid.Parse($"{seed:X8}-0000-4000-8000-000000000000");

    private static readonly string[] Titulos =
    [
        "No puedo acceder al portal",
        "Error al generar el reporte mensual",
        "Consulta sobre la factura del mes",
        "El sistema se cae al imprimir",
        "No recibo correos de notificación",
        "Problema con el cambio de contraseña",
        "Solicitud de permiso especial",
        "El módulo de ventas no carga",
        "Duda sobre los plazos de entrega",
        "Error 500 al guardar un cliente",
        "No me llega el token de acceso",
        "La pantalla se congela al filtrar",
        "Cambio de datos de la empresa",
        "Reporte de saldos incorrecto",
        "El sistema marca vencida una compra",
        "No puedo adjuntar documentos",
        "Consulta de estado de mi solicitud",
        "Error al sincronizar dispositivos",
        "El correo de bienvenida no llega",
        "Problemas con la firma digital",
        "La base de datos parece lenta",
        "Duda sobre licencias de usuario",
        "Error al exportar a Excel",
        "No puedo cambiar mi avatar",
        "El menú principal no aparece",
    ];

    private static readonly string[] Descripciones =
    [
        "Al intentar realizar la operación el sistema muestra un error y no permite continuar con el proceso.",
        "Necesito orientación sobre el procedimiento correcto para completar este trámite internamente.",
        "El comportamiento observado no coincide con el esperado y bloquea mi trabajo diario desde esta mañana.",
        "Reporto la incidencia con todos los detalles disponibles para que el equipo pueda darle pronta atención.",
    ];

    private static EspecSolicitud[] EspecificacionesNorte()
        =>
        [
            // Nueva — 5
            (EstadoSolicitud.Nueva, Prioridad.Baja, 0, null, User1NorteId, 1),
            (EstadoSolicitud.Nueva, Prioridad.Media, 1, null, User2NorteId, 6),
            (EstadoSolicitud.Nueva, Prioridad.Alta, 2, null, User1NorteId, 11),
            (EstadoSolicitud.Nueva, Prioridad.Critica, 3, null, User2NorteId, 16),
            (EstadoSolicitud.Nueva, Prioridad.Media, 0, null, User1NorteId, 21),
            // Asignada — 5
            (EstadoSolicitud.Asignada, Prioridad.Alta, 1, Agente1NorteId, User2NorteId, 26),
            (EstadoSolicitud.Asignada, Prioridad.Media, 2, Agente2NorteId, User1NorteId, 31),
            (EstadoSolicitud.Asignada, Prioridad.Baja, 3, Agente1NorteId, User2NorteId, 36),
            (EstadoSolicitud.Asignada, Prioridad.Critica, 0, AdminNorteId, User1NorteId, 41),
            (EstadoSolicitud.Asignada, Prioridad.Alta, 2, Agente2NorteId, User2NorteId, 46),
            // EnProceso — 4
            (EstadoSolicitud.EnProceso, Prioridad.Critica, 1, Agente1NorteId, User1NorteId, 51),
            (EstadoSolicitud.EnProceso, Prioridad.Baja, 3, Agente2NorteId, User2NorteId, 56),
            (EstadoSolicitud.EnProceso, Prioridad.Media, 0, AdminNorteId, User1NorteId, 61),
            (EstadoSolicitud.EnProceso, Prioridad.Alta, 1, Agente1NorteId, User2NorteId, 66),
            // Resuelta — 4
            (EstadoSolicitud.Resuelta, Prioridad.Media, 2, Agente2NorteId, User1NorteId, 71),
            (EstadoSolicitud.Resuelta, Prioridad.Alta, 0, Agente1NorteId, User2NorteId, 76),
            (EstadoSolicitud.Resuelta, Prioridad.Baja, 1, AdminNorteId, User1NorteId, 81),
            (EstadoSolicitud.Resuelta, Prioridad.Critica, 3, Agente2NorteId, User2NorteId, 86),
            // Cerrada — 4
            (EstadoSolicitud.Cerrada, Prioridad.Alta, 2, Agente1NorteId, User1NorteId, 91),
            (EstadoSolicitud.Cerrada, Prioridad.Media, 0, Agente2NorteId, User2NorteId, 96),
            (EstadoSolicitud.Cerrada, Prioridad.Critica, 1, AdminNorteId, User1NorteId, 101),
            (EstadoSolicitud.Cerrada, Prioridad.Baja, 3, Agente1NorteId, User2NorteId, 106),
            // Cancelada — 3
            (EstadoSolicitud.Cancelada, Prioridad.Media, 0, null, User1NorteId, 111),
            (EstadoSolicitud.Cancelada, Prioridad.Alta, 2, Agente2NorteId, User2NorteId, 116),
            (EstadoSolicitud.Cancelada, Prioridad.Baja, 1, null, User1NorteId, 121),
        ];

    private static EspecSolicitud[] EspecificacionesSur()
        =>
        [
            (EstadoSolicitud.Nueva, Prioridad.Media, 1, null, User1SurId, 2),
            (EstadoSolicitud.Nueva, Prioridad.Baja, 3, null, User1SurId, 12),
            (EstadoSolicitud.Asignada, Prioridad.Alta, 0, AdminSurId, User1SurId, 22),
            (EstadoSolicitud.EnProceso, Prioridad.Critica, 1, AdminSurId, User1SurId, 32),
            (EstadoSolicitud.Resuelta, Prioridad.Media, 2, AdminSurId, User1SurId, 42),
            (EstadoSolicitud.Resuelta, Prioridad.Baja, 3, AdminSurId, User1SurId, 52),
            (EstadoSolicitud.Cerrada, Prioridad.Alta, 0, AdminSurId, User1SurId, 62),
            (EstadoSolicitud.Cancelada, Prioridad.Media, 1, null, User1SurId, 72),
        ];
}
