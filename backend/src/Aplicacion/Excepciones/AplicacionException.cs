namespace Aplicacion.Excepciones;

public abstract class AplicacionException : Exception
{
    public string Codigo { get; }

    protected AplicacionException(string codigo, string message)
        : base(message)
    {
        Codigo = codigo;
    }
}
