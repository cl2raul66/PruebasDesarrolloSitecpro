using Api.Auth;
using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/solicitudes")]
[Authorize]
public sealed class SolicitudesController(
    SolicitudService solicitudes,
    SolicitudMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SolicitudPaginada>> Listar(
        [FromQuery] EstadoSolicitud? estado,
        [FromQuery] Prioridad? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null)
    {
        var request = new SolicitudListaRequest
        {
            Estado = estado,
            Prioridad = prioridad,
            CategoriaId = categoriaId,
            AgenteId = agenteId,
            Q = q,
            Vencidas = vencidas,
            Page = page,
            PageSize = pageSize,
            Sort = string.IsNullOrWhiteSpace(sort) ? "-fechaCreacion" : sort,
        };

        var resultado = await solicitudes.ListarAsync(
            User.TenantId(), User.UserId(), User.Rol(), request);

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<SolicitudDetalleResponse>> Crear([FromBody] SolicitudCreateRequest request)
    {
        var tenantId = User.TenantId();
        var creada = await solicitudes.CrearAsync(tenantId, User.UserId(), request);

        var detalle = await DetalleActualizado(creada.Id);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, detalle);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SolicitudDetalleResponse>> Obtener(Guid id)
        => Ok(await DetalleActualizado(id));

    [HttpPut("{id}")]
    public async Task<ActionResult<SolicitudDetalleResponse>> Editar(Guid id, [FromBody] SolicitudEditRequest request)
    {
        await solicitudes.EditarAsync(id, User.TenantId(), User.UserId(), User.Rol(), request);
        return Ok(await DetalleActualizado(id));
    }

    [HttpPost("{id}/transiciones")]
    public async Task<ActionResult<SolicitudDetalleResponse>> Transicion(Guid id, [FromBody] SolicitudTransicionRequest request)
    {
        await solicitudes.EjecutarTransicionAsync(id, User.TenantId(), User.UserId(), User.Rol(), request);
        return Ok(await DetalleActualizado(id));
    }

    private async Task<SolicitudDetalleResponse> DetalleActualizado(Guid id)
    {
        var solicitud = await solicitudes.ObtenerAsync(
            id, User.TenantId(), User.UserId(), User.Rol());
        return mapper.ToDetalle(solicitud);
    }
}
