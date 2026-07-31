using System.Text.Json.Serialization;

namespace Api.Middleware;

public sealed record ProblemaRespuesta(
    string Type,
    string Title,
    int Status,
    string Detail,
    string Codigo,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, string[]>? Errores = null);

public static class ErrorFactory
{
    public static ProblemaRespuesta Crear(
        string codigo,
        string? detail = null,
        IReadOnlyDictionary<string, string[]>? errores = null)
    {
        var (status, title, slug) = codigo switch
        {
            "NO_AUTENTICADO" => (401, "No autenticado", "no-autenticado"),
            "OPERACION_NO_PERMITIDA" => (403, "Operación no permitida", "operacion-no-permitida"),
            "RECURSO_NO_ENCONTRADO" => (404, "Recurso no encontrado", "recurso-no-encontrado"),
            "TRANSICION_INVALIDA" => (409, "Transición inválida", "transicion-invalida"),
            "AGENTE_INVALIDO" => (422, "Agente inválido", "agente-invalido"),
            "MOTIVO_REQUERIDO" => (422, "Motivo requerido", "motivo-requerido"),
            "PARAMETRO_INVALIDO" => (400, "Parámetro inválido", "parametro-invalido"),
            "VALIDACION" => (422, "Error de validación", "validacion"),
            _ => (500, "Error interno", "error-interno"),
        };

        return new ProblemaRespuesta(
            $"https://mesasitec.local/errores/{slug}",
            title,
            status,
            detail ?? title,
            codigo,
            errores);
    }
}
