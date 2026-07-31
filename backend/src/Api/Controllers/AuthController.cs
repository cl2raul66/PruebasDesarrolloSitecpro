using Api.Auth;
using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class AuthController(AuthService authService, JwtTokenFactory jwtFactory) : ControllerBase
{
    [HttpPost("auth/login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var (usuario, tenantNombre) = await authService.LoginAsync(request.Email, request.Password);
        var (token, expiraEn) = jwtFactory.Generar(usuario, tenantNombre);

        return Ok(new LoginResponse
        {
            AccessToken = token,
            ExpiraEn = expiraEn,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                TenantId = usuario.TenantId,
                TenantNombre = tenantNombre,
            },
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> Me(
        [FromServices] Aplicacion.Interfaces.ITenantRepository tenants,
        [FromServices] Aplicacion.Interfaces.IUsuarioRepository usuarios)
    {
        var userId = User.UserId();
        var tenantId = User.TenantId();

        var usuario = await usuarios.GetByIdAsync(userId, tenantId);
        var tenant = await tenants.GetByIdAsync(tenantId);

        if (usuario is null || tenant is null || !tenant.Activo)
        {
            throw new Aplicacion.Excepciones.NoAutenticadoException("El usuario o la organización ya no están activos.");
        }

        return Ok(new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol,
            TenantId = usuario.TenantId,
            TenantNombre = tenant.Nombre,
        });
    }
}
