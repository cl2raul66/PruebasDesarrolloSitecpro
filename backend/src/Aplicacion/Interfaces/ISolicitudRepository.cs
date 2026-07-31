using Aplicacion.DTOs;
using Dominio;

namespace Aplicacion.Interfaces;

public interface ISolicitudRepository
{
    Task<Solicitud?> GetByIdAsync(Guid id, Guid tenantId);
    Task<Solicitud> AddAsync(Solicitud solicitud);
    Task UpdateAsync(Solicitud solicitud);
    Task<int> ObtenerMaximoCorrelativoAsync(Guid tenantId, int anio);
    Task<SolicitudListadoResultado> ListarAsync(SolicitudFiltros filtros);
}
