namespace Dominio.Excepciones;

public abstract class DominioException : Exception
{
    public string Codigo { get; }

    protected DominioException(string codigo, string message)
        : base(message)
    {
        Codigo = codigo;
    }
}
