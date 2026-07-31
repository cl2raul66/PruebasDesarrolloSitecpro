using Aplicacion.Interfaces;
using Dominio;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories;

public sealed class TenantRepository(MesaSitecDbContext db) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id)
        => db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
}
