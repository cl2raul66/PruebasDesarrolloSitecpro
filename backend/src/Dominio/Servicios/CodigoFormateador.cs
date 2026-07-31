namespace Dominio.Servicios;

public sealed class CodigoFormateador
{
    public string Formatear(int anio, int correlativo)
        => $"SOL-{anio}-{correlativo:D5}";

    public int ExtraerCorrelativo(string codigo)
        => int.Parse(codigo.Split('-')[2]);
}
