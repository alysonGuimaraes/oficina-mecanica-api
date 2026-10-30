using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Materials;

namespace AutoGarage.CommonTestUtilities.Repositories.Materials;

/// <summary>
/// Repositório em memória. Nome pelo valor normalizado, como no repositório real.
/// </summary>
public class FakeMaterialRepository : IMaterialReadOnlyRepository, IMaterialWriteOnlyRepository
{
    private readonly List<Material> _materials = [];

    public IReadOnlyList<Material> Materials => _materials;

    public FakeMaterialRepository WithMaterial(Material material)
    {
        _materials.Add(material);
        return this;
    }

    public Task<bool> ExistsByName(string name, CancellationToken cancellationToken)
    {
        var exists = _materials.Any(m => m.Name == name);

        return Task.FromResult(exists);
    }

    public Task Add(Material material, CancellationToken cancellationToken)
    {
        _materials.Add(material);

        return Task.CompletedTask;
    }
}
