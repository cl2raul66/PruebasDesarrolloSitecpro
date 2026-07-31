using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Dominio;
using Dominio.Servicios;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories;

public sealed class SolicitudRepository(
    MesaSitecDbContext db,
    SlaCalculator slaCalculator,
    CodigoFormateador codigoFormateador) : ISolicitudRepository
{
    public async Task<Solicitud?> GetByIdAsync(Guid id, Guid tenantId)
        => await db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Solicitante)
            .Include(s => s.Agente)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

    public async Task<Solicitud> AddAsync(Solicitud solicitud)
    {
        await db.Solicitudes.AddAsync(solicitud);
        await db.SaveChangesAsync();
        return solicitud;
    }

    public async Task UpdateAsync(Solicitud solicitud)
    {
        db.Solicitudes.Update(solicitud);
        await db.SaveChangesAsync();
    }

    public async Task<int> ObtenerMaximoCorrelativoAsync(Guid tenantId, int anio)
    {
        var codigos = await db.Solicitudes
            .Where(s => s.TenantId == tenantId && s.FechaCreacion.Year == anio)
            .Select(s => s.Codigo)
            .ToListAsync();

        var maximo = 0;
        foreach (var codigo in codigos)
        {
            if (codigoFormateador.ExtraerCorrelativo(codigo) is var correlativo && correlativo > maximo)
            {
                maximo = correlativo;
            }
        }

        return maximo;
    }

    public async Task<SolicitudListadoResultado> ListarAsync(SolicitudFiltros filtros)
    {
        var ahora = DateTime.UtcNow;
        var query = db.Solicitudes.Where(s => s.TenantId == filtros.TenantId);

        if (filtros.SolicitanteId.HasValue)
        {
            query = query.Where(s => s.SolicitanteId == filtros.SolicitanteId);
        }

        if (filtros.Estado.HasValue)
        {
            query = query.Where(s => s.Estado == filtros.Estado);
        }

        if (filtros.Prioridad.HasValue)
        {
            query = query.Where(s => s.Prioridad == filtros.Prioridad);
        }

        if (filtros.CategoriaId.HasValue)
        {
            query = query.Where(s => s.CategoriaId == filtros.CategoriaId);
        }

        if (filtros.AgenteId.HasValue)
        {
            query = query.Where(s => s.AgenteId == filtros.AgenteId);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Q))
        {
            var q = filtros.Q.Trim();
            query = query.Where(s =>
                s.Titulo.Contains(q) || s.Descripcion.Contains(q) || s.Codigo.Contains(q));
        }

        if (filtros.Vencidas.HasValue)
        {
            var vencidas = filtros.Vencidas.Value;
            query = query.Where(s =>
                vencidas
                    ? s.FechaLimiteSla < ahora
                      && s.Estado != EstadoSolicitud.Resuelta
                      && s.Estado != EstadoSolicitud.Cerrada
                      && s.Estado != EstadoSolicitud.Cancelada
                    : !(s.FechaLimiteSla < ahora
                        && s.Estado != EstadoSolicitud.Resuelta
                        && s.Estado != EstadoSolicitud.Cerrada
                        && s.Estado != EstadoSolicitud.Cancelada));
        }

        var total = await query.CountAsync();

        query = AplicarOrdenamiento(query, filtros.Sort);

        var paginas = await query
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Skip((filtros.Page - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .ToListAsync();

        var items = paginas.Select(s => new SolicitudListItem
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Estado = s.Estado,
            Prioridad = s.Prioridad,
            Categoria = new CategoriaResumen(s.Categoria.Id, s.Categoria.Nombre),
            Agente = s.Agente is null ? null : new UsuarioResumen(s.Agente.Id, s.Agente.Nombre),
            FechaCreacion = s.FechaCreacion,
            FechaLimiteSla = s.FechaLimiteSla,
            Vencida = slaCalculator.EstaVencida(s.FechaLimiteSla, s.Estado),
        }).ToList();

        return new SolicitudListadoResultado { Items = items, Total = total };
    }

    private static IQueryable<Solicitud> AplicarOrdenamiento(IQueryable<Solicitud> query, string sort)
    {
        // Prioridad enum declared as Baja=0, Media=1, Alta=2, Critica=3,
        // por lo que el orden por valor del enum coincide con el orden semántico requerido.
        return sort switch
        {
            "fechaCreacion" => query.OrderBy(s => s.FechaCreacion),
            "prioridad" => query.OrderBy(s => s.Prioridad),
            "-prioridad" => query.OrderByDescending(s => s.Prioridad),
            "codigo" => query.OrderBy(s => s.Codigo),
            _ => query.OrderByDescending(s => s.FechaCreacion),
        };
    }
}
