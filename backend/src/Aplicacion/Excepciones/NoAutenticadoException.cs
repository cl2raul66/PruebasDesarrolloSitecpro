namespace Aplicacion.Excepciones;

public sealed class NoAutenticadoException(string message)
    : AplicacionException("NO_AUTENTICADO", message);
