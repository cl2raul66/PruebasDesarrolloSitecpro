using Aplicacion.Servicios;
using Dominio;

namespace Tests;

public class PermissionServiceTests
{
    private readonly PermissionService _permisos = new();
    private readonly Guid _solicitante = Guid.NewGuid();
    private readonly Guid _otroUsuario = Guid.NewGuid();

    [Fact]
    public void Solicitante_NoPuedeVerDetalleDeAjena()
    {
        Assert.False(_permisos.PuedoVerDetalle(Rol.Solicitante, _solicitante, _otroUsuario));
    }

    [Fact]
    public void Solicitante_PuedeVerDetalleDePropia()
    {
        Assert.True(_permisos.PuedoVerDetalle(Rol.Solicitante, _solicitante, _solicitante));
    }

    [Fact]
    public void AdminYPuedeVerDetalleDeCualquiera()
    {
        Assert.True(_permisos.PuedoVerDetalle(Rol.Admin, _solicitante, _otroUsuario));
        Assert.True(_permisos.PuedoVerDetalle(Rol.Agente, _solicitante, _otroUsuario));
    }

    [Fact]
    public void Solicitante_PuedeEditarPropiaEnNueva()
    {
        Assert.True(_permisos.PuedoEditar(Rol.Solicitante, EstadoSolicitud.Nueva, _solicitante, _solicitante));
    }

    [Fact]
    public void Solicitante_NoPuedeEditarPropiaFueraDeNueva()
    {
        Assert.False(_permisos.PuedoEditar(Rol.Solicitante, EstadoSolicitud.Asignada, _solicitante, _solicitante));
        Assert.False(_permisos.PuedoEditar(Rol.Solicitante, EstadoSolicitud.EnProceso, _solicitante, _solicitante));
    }

    [Fact]
    public void Solicitante_NoPuedeEditarAjena()
    {
        Assert.False(_permisos.PuedoEditar(Rol.Solicitante, EstadoSolicitud.Nueva, _solicitante, _otroUsuario));
    }

    [Fact]
    public void AdminYAgente_PuedenEditarEnCualquierEstado()
    {
        Assert.True(_permisos.PuedoEditar(Rol.Admin, EstadoSolicitud.Asignada, _solicitante, _otroUsuario));
        Assert.True(_permisos.PuedoEditar(Rol.Agente, EstadoSolicitud.EnProceso, _solicitante, _otroUsuario));
    }

    [Fact]
    public void Solicitante_PuedeCerrarSoloSolicitudesPropias()
    {
        Assert.True(_permisos.PuedoEjecutarTransicion(Rol.Solicitante, "cerrar", _solicitante, _solicitante));
        Assert.False(_permisos.PuedoEjecutarTransicion(Rol.Solicitante, "cerrar", _solicitante, _otroUsuario));
    }

    [Fact]
    public void Solicitante_NoPuedeEjecutarAccionesDeEquipo()
    {
        Assert.False(_permisos.PuedoEjecutarTransicion(Rol.Solicitante, "asignar", _solicitante, _solicitante));
        Assert.False(_permisos.PuedoEjecutarTransicion(Rol.Solicitante, "iniciar", _solicitante, _solicitante));
        Assert.False(_permisos.PuedoEjecutarTransicion(Rol.Solicitante, "resolver", _solicitante, _solicitante));
        Assert.False(_permisos.PuedoEjecutarTransicion(Rol.Solicitante, "reabrir", _solicitante, _solicitante));
        Assert.False(_permisos.PuedoEjecutarTransicion(Rol.Solicitante, "cancelar", _solicitante, _solicitante));
    }

    [Fact]
    public void Agente_NoPuedeCancelar()
    {
        Assert.False(_permisos.PuedoEjecutarTransicion(Rol.Agente, "cancelar", _solicitante, _otroUsuario));
    }

    [Fact]
    public void Agente_PuedeEjecutarLasDemasAcciones()
    {
        Assert.True(_permisos.PuedoEjecutarTransicion(Rol.Agente, "asignar", _solicitante, _otroUsuario));
        Assert.True(_permisos.PuedoEjecutarTransicion(Rol.Agente, "iniciar", _solicitante, _otroUsuario));
        Assert.True(_permisos.PuedoEjecutarTransicion(Rol.Agente, "resolver", _solicitante, _otroUsuario));
        Assert.True(_permisos.PuedoEjecutarTransicion(Rol.Agente, "reabrir", _solicitante, _otroUsuario));
        Assert.True(_permisos.PuedoEjecutarTransicion(Rol.Agente, "cerrar", _solicitante, _otroUsuario));
    }

    [Fact]
    public void Admin_PuedeCancelar()
    {
        Assert.True(_permisos.PuedoEjecutarTransicion(Rol.Admin, "cancelar", _solicitante, _otroUsuario));
    }

    [Fact]
    public void Solicitante_NoPuedeListarTodas()
    {
        Assert.False(_permisos.PuedoListar(Rol.Solicitante));
    }

    [Fact]
    public void AdminYAgente_PuedenListarTodas()
    {
        Assert.True(_permisos.PuedoListar(Rol.Admin));
        Assert.True(_permisos.PuedoListar(Rol.Agente));
    }
}
