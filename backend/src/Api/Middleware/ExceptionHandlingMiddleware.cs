using Aplicacion.Excepciones;
using Dominio.Excepciones;

namespace Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var problema = Resolver(ex);

            if (problema.Status >= 500)
            {
                logger.LogError(ex, "Excepción no controlada: {Mensaje}", ex.Message);
            }

            context.Response.StatusCode = problema.Status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problema);
        }
    }

    private static ProblemaRespuesta Resolver(Exception ex)
    {
        return ex switch
        {
            ValidacionException ve => ErrorFactory.Crear("VALIDACION", ve.Message, ve.Errores),
            ParametroInvalidoException pi => ErrorFactory.Crear(
                "PARAMETRO_INVALIDO", pi.Message,
                new Dictionary<string, string[]> { ["parametros"] = [pi.Message] }),
            BadHttpRequestException => new ProblemaRespuesta(
                "https://mesasitec.local/errores/validacion",
                "Error de validación",
                StatusCodes.Status400BadRequest,
                "El cuerpo de la petición no es válido.",
                "VALIDACION"),
            DominioException d => ErrorFactory.Crear(d.Codigo, d.Message),
            AplicacionException a => ErrorFactory.Crear(a.Codigo, a.Message),
            _ => ErrorFactory.Crear("ERROR_INTERNO"),
        };
    }
}
