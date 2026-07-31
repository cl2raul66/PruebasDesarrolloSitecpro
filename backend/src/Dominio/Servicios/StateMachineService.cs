using Dominio.Excepciones;

namespace Dominio.Servicios;

public sealed class StateMachineService
{
    private static readonly Dictionary<(EstadoSolicitud Estado, string Accion), EstadoSolicitud> _transiciones = new()
    {
        { (EstadoSolicitud.Nueva, "asignar"), EstadoSolicitud.Asignada },
        { (EstadoSolicitud.Nueva, "cancelar"), EstadoSolicitud.Cancelada },
        { (EstadoSolicitud.Asignada, "iniciar"), EstadoSolicitud.EnProceso },
        { (EstadoSolicitud.Asignada, "asignar"), EstadoSolicitud.Asignada },
        { (EstadoSolicitud.Asignada, "cancelar"), EstadoSolicitud.Cancelada },
        { (EstadoSolicitud.EnProceso, "resolver"), EstadoSolicitud.Resuelta },
        { (EstadoSolicitud.EnProceso, "asignar"), EstadoSolicitud.Asignada },
        { (EstadoSolicitud.EnProceso, "cancelar"), EstadoSolicitud.Cancelada },
        { (EstadoSolicitud.Resuelta, "cerrar"), EstadoSolicitud.Cerrada },
        { (EstadoSolicitud.Resuelta, "reabrir"), EstadoSolicitud.EnProceso },
    };

    public bool PuedeTransicionar(EstadoSolicitud estado, string accion)
        => _transiciones.ContainsKey((estado, accion));

    public EstadoSolicitud Transicionar(EstadoSolicitud estado, string accion)
    {
        if (_transiciones.TryGetValue((estado, accion), out var siguiente))
        {
            return siguiente;
        }

        throw new TransicionInvalidaException(accion, estado);
    }
}
