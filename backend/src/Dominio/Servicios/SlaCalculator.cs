namespace Dominio.Servicios;

public sealed class SlaCalculator
{
    private static readonly IReadOnlyDictionary<Prioridad, double> _factores = new Dictionary<Prioridad, double>
    {
        { Prioridad.Critica, 0.5 },
        { Prioridad.Alta, 0.75 },
        { Prioridad.Media, 1.0 },
        { Prioridad.Baja, 2.0 },
    };

    public DateTime Calcular(DateTime fechaCreacion, int slaHoras, Prioridad prioridad)
        => fechaCreacion.AddHours(slaHoras * _factores[prioridad]);

    public bool EstaVencida(DateTime fechaLimiteSla, EstadoSolicitud estado)
        => fechaLimiteSla < DateTime.UtcNow
           && estado is not (EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada or EstadoSolicitud.Cancelada);
}
