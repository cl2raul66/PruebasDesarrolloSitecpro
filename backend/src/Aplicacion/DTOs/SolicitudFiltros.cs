using Dominio;

namespace Aplicacion.DTOs;

public sealed record SolicitudFiltros
{
    public Guid TenantId { get; init; }
    public Guid? SolicitanteId { get; init; }
    public EstadoSolicitud? Estado { get; init; }
    public Prioridad? Prioridad { get; init; }
    public Guid? CategoriaId { get; init; }
    public Guid? AgenteId { get; init; }
    public string? Q { get; init; }
    public bool? Vencidas { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string Sort { get; init; } = "-fechaCreacion";
}

public sealed record SolicitudListadoResultado
{
    public IReadOnlyList<SolicitudListItem> Items { get; init; } = [];
    public int Total { get; init; }
}
