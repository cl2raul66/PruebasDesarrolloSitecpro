using Dominio;
using Dominio.Servicios;

namespace Tests;

public class SlaCalculatorTests
{
    private readonly SlaCalculator _calculator = new();

    [Fact]
    public void Calcular_IncidenteCritica_DevuelveCuatroHoras()
    {
        var baseFecha = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        var resultado = _calculator.Calcular(baseFecha, slaHoras: 8, Prioridad.Critica);

        Assert.Equal(baseFecha.AddHours(4), resultado);
    }

    [Fact]
    public void Calcular_ConsultaBaja_DevuelveCuarentaYOchoHoras()
    {
        var baseFecha = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        var resultado = _calculator.Calcular(baseFecha, slaHoras: 24, Prioridad.Baja);

        Assert.Equal(baseFecha.AddHours(48), resultado);
    }

    [Fact]
    public void Calcular_RequerimientoAlta_DevuelveTreintaHoras()
    {
        var baseFecha = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        var resultado = _calculator.Calcular(baseFecha, slaHoras: 40, Prioridad.Alta);

        Assert.Equal(baseFecha.AddHours(30), resultado);
    }

    [Fact]
    public void EstaVencida_LimitePasado_EstadoNoFinal_DevuelveTrue()
    {
        var limite = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //var ahora = new DateTime(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc);

        Assert.True(_calculator.EstaVencida(limite, EstadoSolicitud.Nueva));
        Assert.True(_calculator.EstaVencida(limite, EstadoSolicitud.Asignada));
        Assert.True(_calculator.EstaVencida(limite, EstadoSolicitud.EnProceso));
    }

    [Fact]
    public void EstaVencida_LimitePasado_EstadoFinal_DevuelveFalse()
    {
        var limite = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //var ahora = new DateTime(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc);

        Assert.False(_calculator.EstaVencida(limite, EstadoSolicitud.Resuelta));
        Assert.False(_calculator.EstaVencida(limite, EstadoSolicitud.Cerrada));
        Assert.False(_calculator.EstaVencida(limite, EstadoSolicitud.Cancelada));
    }

    [Fact]
    public void EstaVencida_LimiteFuturo_DevuelveFalse()
    {
        var limite = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        //var ahora = new DateTime(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc);

        Assert.False(_calculator.EstaVencida(limite, EstadoSolicitud.Nueva));
    }
}
