using Aplicacion.DTOs;
using Dominio;
using Dominio.Servicios;

namespace Aplicacion.Servicios;

public sealed class SolicitudMapper(SlaCalculator slaCalculator)
{
    public SolicitudDetalleResponse ToDetalle(Solicitud s)
        => new()
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Descripcion = s.Descripcion,
            Estado = s.Estado,
            Prioridad = s.Prioridad,
            Categoria = new CategoriaResumen(s.Categoria.Id, s.Categoria.Nombre),
            Solicitante = new UsuarioResumen(s.Solicitante.Id, s.Solicitante.Nombre),
            Agente = s.Agente is null ? null : new UsuarioResumen(s.Agente.Id, s.Agente.Nombre),
            FechaCreacion = s.FechaCreacion,
            FechaLimiteSla = s.FechaLimiteSla,
            FechaResolucion = s.FechaResolucion,
            MotivoResolucion = s.MotivoResolucion,
            MotivoCancelacion = s.MotivoCancelacion,
            Vencida = slaCalculator.EstaVencida(s.FechaLimiteSla, s.Estado),
        };
}
