namespace Dominio.Excepciones;

public sealed class AgenteInvalidoException : DominioException
{
    public AgenteInvalidoException()
        : base("AGENTE_INVALIDO", "El agente indicado no existe, no está activo o no pertenece a la organización.")
    {
    }
}
