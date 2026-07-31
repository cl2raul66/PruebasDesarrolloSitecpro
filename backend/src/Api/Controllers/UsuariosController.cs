using Api.Auth;
using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize]
public sealed class UsuariosController(UsuarioService usuarioService) : ControllerBase
{
    /// <summary>
    /// Lista de agentes (roles Agente/Admin) activos de la organización. Necesario
    /// para el selector de asignación del frontend (RN-05). No es parte del contrato
    /// obligatorio, es una adición declarada en DECISIONES.md.
    /// </summary>
    [HttpGet("agentes")]
    public async Task<ActionResult<IReadOnlyList<UsuarioResumen>>> Agentes()
    {
        var agentes = await usuarioService.ListarAgentesActivosAsync(User.TenantId());
        return Ok(agentes);
    }
}
