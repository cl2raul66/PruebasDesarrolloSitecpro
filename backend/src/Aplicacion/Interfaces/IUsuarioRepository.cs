using Dominio;

namespace Aplicacion.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, Guid tenantId);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IReadOnlyList<Usuario>> GetAgentesActivosAsync(Guid tenantId);
}
