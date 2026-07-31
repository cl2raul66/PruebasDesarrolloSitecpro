using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Dominio;

namespace Aplicacion.Servicios;

public sealed class UsuarioService(IUsuarioRepository usuarios)
{
    public async Task<IReadOnlyList<UsuarioResumen>> ListarAgentesActivosAsync(Guid tenantId)
    {
        var agentes = await usuarios.GetAgentesActivosAsync(tenantId);
        return agentes
            .Select(u => new UsuarioResumen(u.Id, u.Nombre))
            .ToList();
    }
}
