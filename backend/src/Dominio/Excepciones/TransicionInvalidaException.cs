namespace Dominio.Excepciones;

public sealed class TransicionInvalidaException : DominioException
{
    public TransicionInvalidaException(string accion, EstadoSolicitud estado)
        : base("TRANSICION_INVALIDA", $"No se puede aplicar '{accion}' sobre una solicitud en estado '{estado}'.")
    {
    }
}
