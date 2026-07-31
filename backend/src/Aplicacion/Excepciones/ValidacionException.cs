namespace Aplicacion.Excepciones;

public sealed class ValidacionException : AplicacionException
{
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    public ValidacionException(IReadOnlyDictionary<string, string[]> errores)
        : base("VALIDACION", "La solicitud no cumple con los requisitos de validación.")
    {
        Errores = errores;
    }
}
