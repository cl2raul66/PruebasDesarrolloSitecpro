using Dominio;
using Dominio.Excepciones;
using Dominio.Servicios;

namespace Tests;

public class StateMachineServiceTests
{
    private readonly StateMachineService _service = new();

    [Fact]
    public void Transicionar_Nueva_Asignar_DevuelveAsignada()
    {
        var resultado = _service.Transicionar(EstadoSolicitud.Nueva, "asignar");

        Assert.Equal(EstadoSolicitud.Asignada, resultado);
    }

    [Fact]
    public void Transicionar_Nueva_Resolver_LanzaTransicionInvalida()
    {
        var ex = Assert.Throws<TransicionInvalidaException>(
            () => _service.Transicionar(EstadoSolicitud.Nueva, "resolver"));

        Assert.Equal("TRANSICION_INVALIDA", ex.Codigo);
        Assert.Equal(
            "No se puede aplicar 'resolver' sobre una solicitud en estado 'Nueva'.",
            ex.Message);
    }

    [Fact]
    public void Transicionar_Asignada_Iniciar_DevuelveEnProceso()
    {
        var resultado = _service.Transicionar(EstadoSolicitud.Asignada, "iniciar");

        Assert.Equal(EstadoSolicitud.EnProceso, resultado);
    }

    [Fact]
    public void Transicionar_Asignada_Asignar_DevuelveAsignada()
    {
        var resultado = _service.Transicionar(EstadoSolicitud.Asignada, "asignar");

        Assert.Equal(EstadoSolicitud.Asignada, resultado);
    }

    [Fact]
    public void Transicionar_Resuelta_Reabrir_DevuelveEnProceso()
    {
        var resultado = _service.Transicionar(EstadoSolicitud.Resuelta, "reabrir");

        Assert.Equal(EstadoSolicitud.EnProceso, resultado);
    }

    [Fact]
    public void Transicionar_Resuelta_Cerrar_DevuelveCerrada()
    {
        var resultado = _service.Transicionar(EstadoSolicitud.Resuelta, "cerrar");

        Assert.Equal(EstadoSolicitud.Cerrada, resultado);
    }

    [Fact]
    public void Transicionar_Cerrada_Asignar_LanzaTransicionInvalida()
    {
        Assert.Throws<TransicionInvalidaException>(
            () => _service.Transicionar(EstadoSolicitud.Cerrada, "asignar"));
    }

    [Fact]
    public void Transicionar_Cancelada_CualquierAccion_LanzaTransicionInvalida()
    {
        Assert.Throws<TransicionInvalidaException>(
            () => _service.Transicionar(EstadoSolicitud.Cancelada, "reabrir"));
    }
}
