namespace Dominio.Excepciones;

public sealed class RecursoNoEncontradoException : DominioException
{
    public RecursoNoEncontradoException(string message)
        : base("RECURSO_NO_ENCONTRADO", message)
    {
    }
}
