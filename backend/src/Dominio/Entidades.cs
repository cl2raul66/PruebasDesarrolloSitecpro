using System.ComponentModel.DataAnnotations;

namespace Dominio;

public class Tenant
{
    public Guid Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; }
}

public class Usuario
{
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public Rol Rol { get; set; }

    public bool Activo { get; set; }
}

public class Categoria
{
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public int SlaHoras { get; set; }

    public bool Activo { get; set; }
}

public class Solicitud
{
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 5)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid CategoriaId { get; set; }

    [Required]
    public Prioridad Prioridad { get; set; }

    [Required]
    public EstadoSolicitud Estado { get; set; }

    [Required]
    public Guid SolicitanteId { get; set; }

    public Guid? AgenteId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaLimiteSla { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string? MotivoResolucion { get; set; }

    public string? MotivoCancelacion { get; set; }
}
