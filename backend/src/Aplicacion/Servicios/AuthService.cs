using Aplicacion.DTOs;
using Aplicacion.Excepciones;
using Aplicacion.Interfaces;
using Dominio;

namespace Aplicacion.Servicios;

public sealed class AuthService(IUsuarioRepository usuarios, ITenantRepository tenants)
{
    public async Task<(Usuario Usuario, string TenantNombre)> LoginAsync(string email, string password)
    {
        var correo = email?.Trim().ToLowerInvariant() ?? string.Empty;

        var usuario = await usuarios.GetByEmailAsync(correo);
        if (usuario is null
            || !usuario.Activo
            || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
        {
            throw new NoAutenticadoException("Las credenciales proporcionadas son incorrectas.");
        }

        var tenant = await tenants.GetByIdAsync(usuario.TenantId);
        if (tenant is null || !tenant.Activo)
        {
            throw new NoAutenticadoException("Las credenciales proporcionadas son incorrectas.");
        }

        return (usuario, tenant.Nombre);
    }
}
