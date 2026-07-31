using Dominio;

namespace Aplicacion.DTOs;

public sealed record CategoriaResumen(Guid Id, string Nombre);

public sealed record CategoriaDto(Guid Id, string Nombre, int SlaHoras);

public sealed record UsuarioResumen(Guid Id, string Nombre);

public sealed record SolicitudListItem
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Titulo { get; init; } = string.Empty;
    public EstadoSolicitud Estado { get; init; }
    public Prioridad Prioridad { get; init; }
    public CategoriaResumen Categoria { get; init; } = null!;
    public UsuarioResumen? Agente { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaLimiteSla { get; init; }
    public bool Vencida { get; init; }
}

public sealed record SolicitudPaginada
{
    public IReadOnlyList<SolicitudListItem> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPaginas { get; init; }
}

public sealed record SolicitudDetalleResponse
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public EstadoSolicitud Estado { get; init; }
    public Prioridad Prioridad { get; init; }
    public CategoriaResumen Categoria { get; init; } = null!;
    public UsuarioResumen Solicitante { get; init; } = null!;
    public UsuarioResumen? Agente { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime FechaLimiteSla { get; init; }
    public DateTime? FechaResolucion { get; init; }
    public string? MotivoResolucion { get; init; }
    public string? MotivoCancelacion { get; init; }
    public bool Vencida { get; init; }
}
