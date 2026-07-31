using Aplicacion.Interfaces;
using Dominio;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories;

public sealed class CategoriaRepository(MesaSitecDbContext db) : ICategoriaRepository
{
    public Task<Categoria?> GetByIdAsync(Guid id, Guid tenantId)
        => db.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

    public async Task<IReadOnlyList<Categoria>> GetActivasByTenantAsync(Guid tenantId)
        => await db.Categorias
            .Where(c => c.TenantId == tenantId && c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
}
