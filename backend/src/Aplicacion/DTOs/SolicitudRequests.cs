using Dominio;

namespace Aplicacion.DTOs;

public sealed record SolicitudCreateRequest
{
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public Guid CategoriaId { get; init; }
    public Prioridad Prioridad { get; init; }
}

public sealed record SolicitudEditRequest
{
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public Guid CategoriaId { get; init; }
    public Prioridad Prioridad { get; init; }
}

public sealed record SolicitudTransicionRequest
{
    public string Accion { get; init; } = string.Empty;
    public Guid? AgenteId { get; init; }
    public string? Motivo { get; init; }
}

public sealed record SolicitudListaRequest
{
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
