namespace Dominio;

public enum Rol
{
    Admin,
    Agente,
    Solicitante
}

public enum Prioridad
{
    Baja,
    Media,
    Alta,
    Critica
}

public enum EstadoSolicitud
{
    Nueva,
    Asignada,
    EnProceso,
    Resuelta,
    Cerrada,
    Cancelada
}
