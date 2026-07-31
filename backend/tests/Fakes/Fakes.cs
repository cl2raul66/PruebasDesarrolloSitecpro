using Aplicacion.DTOs;
using Aplicacion.Interfaces;
using Dominio;

namespace Tests.Fakes;

internal sealed class FakeSolicitudRepository : ISolicitudRepository
{
    public List<Solicitud> Data { get; } = [];

    public SolicitudFiltros? UltimoFiltros { get; private set; }

    public Task<Solicitud?> GetByIdAsync(Guid id, Guid tenantId)
        => Task.FromResult(Data.FirstOrDefault(s => s.Id == id && s.TenantId == tenantId));

    public Task<Solicitud> AddAsync(Solicitud solicitud)
    {
        Data.Add(solicitud);
        return Task.FromResult(solicitud);
    }

    public Task UpdateAsync(Solicitud solicitud)
        => Task.CompletedTask;

    public Task<int> ObtenerMaximoCorrelativoAsync(Guid tenantId, int anio)
        => Task.FromResult(Data.Count(s => s.TenantId == tenantId && s.FechaCreacion.Year == anio));

    public Task<SolicitudListadoResultado> ListarAsync(SolicitudFiltros filtros)
    {
        UltimoFiltros = filtros;

        var items = Data
            .Where(s => s.TenantId == filtros.TenantId)
            .Where(s => !filtros.SolicitanteId.HasValue || s.SolicitanteId == filtros.SolicitanteId)
            .Where(s => !filtros.Estado.HasValue || s.Estado == filtros.Estado)
            .Where(s => !filtros.Prioridad.HasValue || s.Prioridad == filtros.Prioridad)
            .Where(s => !filtros.CategoriaId.HasValue || s.CategoriaId == filtros.CategoriaId)
            .Where(s => !filtros.AgenteId.HasValue || s.AgenteId == filtros.AgenteId)
            .Select(s => new SolicitudListItem
            {
                Id = s.Id,
                Codigo = s.Codigo,
                Titulo = s.Titulo,
                Estado = s.Estado,
                Prioridad = s.Prioridad,
                Categoria = new CategoriaResumen(s.CategoriaId, "Incidente"),
                FechaCreacion = s.FechaCreacion,
                FechaLimiteSla = s.FechaLimiteSla,
            })
            .ToList();

        return Task.FromResult(new SolicitudListadoResultado
        {
            Items = items,
            Total = items.Count,
        });
    }
}

internal sealed class FakeCategoriaRepository : ICategoriaRepository
{
    public List<Categoria> Data { get; } = [];

    public Task<Categoria?> GetByIdAsync(Guid id, Guid tenantId)
        => Task.FromResult(Data.FirstOrDefault(c => c.Id == id && c.TenantId == tenantId));

    public Task<IReadOnlyList<Categoria>> GetActivasByTenantAsync(Guid tenantId)
        => Task.FromResult<IReadOnlyList<Categoria>>(
            Data.Where(c => c.TenantId == tenantId && c.Activo).ToList());
}

internal sealed class FakeUsuarioRepository : IUsuarioRepository
{
    public List<Usuario> Data { get; } = [];

    public Task<Usuario?> GetByIdAsync(Guid id, Guid tenantId)
        => Task.FromResult(Data.FirstOrDefault(u => u.Id == id && u.TenantId == tenantId));

    public Task<Usuario?> GetByEmailAsync(string email)
        => Task.FromResult(Data.FirstOrDefault(u => u.Email == email));

    public Task<IReadOnlyList<Usuario>> GetAgentesActivosAsync(Guid tenantId)
        => Task.FromResult<IReadOnlyList<Usuario>>(
            Data.Where(u => u.TenantId == tenantId && u.Activo && u.Rol is Rol.Agente or Rol.Admin).ToList());
}
