using Aplicacion.Interfaces;
using Dominio;
using Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Repositories;

public sealed class UsuarioRepository(MesaSitecDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> GetByIdAsync(Guid id, Guid tenantId)
        => db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);

    public Task<Usuario?> GetByEmailAsync(string email)
        => db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IReadOnlyList<Usuario>> GetAgentesActivosAsync(Guid tenantId)
        => await db.Usuarios
            .Where(u => u.TenantId == tenantId && u.Activo && (u.Rol == Rol.Agente || u.Rol == Rol.Admin))
            .OrderBy(u => u.Nombre)
            .ToListAsync();
}
