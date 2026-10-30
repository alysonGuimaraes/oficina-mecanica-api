using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Repositories.Services;

public interface IServiceReadOnlyRepository
{
    Task<bool> ExistsByName(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<Service>> GetByStatus(EntityStatus status, CancellationToken cancellationToken);
}
