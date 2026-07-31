using Aplicacion.Interfaces;
using Dominio;

namespace Aplicacion.Servicios;

public sealed class CategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaService(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<Categoria>> ListarActivasAsync(Guid tenantId)
        => await _categoriaRepository.GetActivasByTenantAsync(tenantId);
}
