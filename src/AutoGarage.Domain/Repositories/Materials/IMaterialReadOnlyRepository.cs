using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Repositories.Materials;

public interface IMaterialReadOnlyRepository
{
    Task<bool> ExistsByName(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<Material>> GetByStatus(EntityStatus status, CancellationToken cancellationToken);
}
