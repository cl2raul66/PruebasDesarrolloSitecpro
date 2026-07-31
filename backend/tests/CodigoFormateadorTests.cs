using Dominio.Servicios;

namespace Tests;

public class CodigoFormateadorTests
{
    private readonly CodigoFormateador _formateador = new();

    [Theory]
    [InlineData(2026, 1, "SOL-2026-00001")]
    [InlineData(2026, 42, "SOL-2026-00042")]
    [InlineData(2025, 100, "SOL-2025-00100")]
    [InlineData(2024, 99999, "SOL-2024-99999")]
    public void Formatear_GeneraFormatoConCerosALaIzquierda(int anio, int correlativo, string esperado)
    {
        var resultado = _formateador.Formatear(anio, correlativo);

        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void ExtraerCorrelativo_ParseaElCorrelativoNumerico()
    {
        var resultado = _formateador.ExtraerCorrelativo("SOL-2026-00042");

        Assert.Equal(42, resultado);
    }
}
