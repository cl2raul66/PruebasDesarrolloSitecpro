using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dominio;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

public sealed class JwtTokenFactory(IConfiguration configuration)
{
    private int ExpiraEnHoras => configuration.GetValue<int?>("Jwt:ExpirationHours") ?? 8;

    public (string Token, int ExpiraEnSegundos) Generar(Usuario usuario, string tenantNombre)
    {
        var claims = new[]
        {
            new Claim("sub", usuario.Id.ToString()),
            new Claim("tenantId", usuario.TenantId.ToString()),
            new Claim("rol", usuario.Rol.ToString()),
            new Claim("email", usuario.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "MesaSitecApi",
            audience: configuration["Jwt:Audience"] ?? "MesaSitecClients",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(ExpiraEnHoras),
            signingCredentials: credenciales);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, ExpiraEnHoras * 3600);
    }
}
