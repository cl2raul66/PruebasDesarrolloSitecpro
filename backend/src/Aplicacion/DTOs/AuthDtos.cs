using Dominio;

namespace Aplicacion.DTOs;

public sealed record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed record UsuarioDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Rol Rol { get; init; }
    public Guid TenantId { get; init; }
    public string TenantNombre { get; init; } = string.Empty;
}

public sealed record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public int ExpiraEn { get; init; }
    public UsuarioDto Usuario { get; init; } = null!;
}
