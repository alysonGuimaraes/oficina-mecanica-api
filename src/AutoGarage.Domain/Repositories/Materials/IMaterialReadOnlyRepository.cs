using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Repositories.Materials;

public interface IMaterialReadOnlyRepository
{
    Task<bool> ExistsByName(string name, CancellationToken cancellationToken);

    Task<bool> ExistsByNameExcludingId(string name, Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Material>> GetByStatus(EntityStatus status, CancellationToken cancellationToken);

    Task<Material?> GetById(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Material>> GetLowStockByStatus(EntityStatus status, CancellationToken cancellationToken);
}
