using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.Materials;

public interface IMaterialWriteOnlyRepository
{
    Task Add(Material material, CancellationToken cancellationToken);

    Task Update(Material material, CancellationToken cancellationToken);
}
