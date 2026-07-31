namespace Aplicacion.Excepciones;

public sealed class ParametroInvalidoException : AplicacionException
{
    public ParametroInvalidoException(string message)
        : base("PARAMETRO_INVALIDO", message)
    {
    }
}
