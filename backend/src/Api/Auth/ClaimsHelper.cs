using System.Security.Claims;
using Aplicacion.Excepciones;
using Dominio;

namespace Api.Auth;

public static class ClaimsHelper
{
    public static Guid UserId(this ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue("sub"), out var id)
            ? id
            : throw new NoAutenticadoException("El token no contiene un identificador de usuario válido.");

    public static Guid TenantId(this ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue("tenantId"), out var id)
            ? id
            : throw new NoAutenticadoException("El token no contiene un tenantId válido.");

    public static Rol Rol(this ClaimsPrincipal user)
        => Enum.TryParse<Rol>(user.FindFirstValue("rol"), out var rol)
            ? rol
            : throw new NoAutenticadoException("El token no contiene un rol válido.");

    public static string Email(this ClaimsPrincipal user)
        => user.FindFirstValue("email") ?? throw new NoAutenticadoException("El token no contiene un email válido.");
}
