namespace Dominio.Excepciones;

public sealed class MotivoRequeridoException : DominioException
{
    public MotivoRequeridoException(string message)
        : base("MOTIVO_REQUERIDO", message)
    {
    }
}
