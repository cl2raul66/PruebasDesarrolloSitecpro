using Api.Auth;
using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize]
public sealed class CategoriasController(CategoriaService categoriaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar()
    {
        var tenantId = User.TenantId();
        var categorias = await categoriaService.ListarActivasAsync(tenantId);

        return Ok(categorias.Select(c => new CategoriaDto(c.Id, c.Nombre, c.SlaHoras)));
    }
}
