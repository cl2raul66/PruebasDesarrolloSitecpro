namespace Dominio.Excepciones;

public sealed class PermisoDenegadoException : DominioException
{
    public PermisoDenegadoException(string message)
        : base("OPERACION_NO_PERMITIDA", message)
    {
    }
}
