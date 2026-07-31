using Dominio;

namespace Aplicacion.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IReadOnlyList<Categoria>> GetActivasByTenantAsync(Guid tenantId);
}
