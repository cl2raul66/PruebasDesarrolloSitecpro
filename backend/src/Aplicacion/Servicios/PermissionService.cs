using Dominio;

namespace Aplicacion.Servicios;

public sealed class PermissionService
{
    public bool PuedoListar(Rol rol)
        => rol != Rol.Solicitante;

    public bool PuedoVerDetalle(Rol rol, Guid solicitanteId, Guid userId)
        => rol == Rol.Solicitante ? solicitanteId == userId : true;

    public bool PuedoEditar(Rol rol, EstadoSolicitud estado, Guid solicitanteId, Guid userId)
        => rol switch
        {
            Rol.Admin or Rol.Agente => true,
            Rol.Solicitante => solicitanteId == userId && estado == EstadoSolicitud.Nueva,
            _ => false,
        };

    public bool PuedoEjecutarTransicion(Rol rol, string accion, Guid solicitanteId, Guid userId)
        => rol switch
        {
            Rol.Admin => true,
            Rol.Agente => accion != "cancelar",
            Rol.Solicitante => accion == "cerrar" && solicitanteId == userId,
            _ => false,
        };
}
