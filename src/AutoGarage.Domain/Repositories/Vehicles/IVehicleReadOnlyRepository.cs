using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Repositories.Vehicles;

public interface IVehicleReadOnlyRepository
{
    Task<bool> ExistsByPlate(string plate, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vehicle>> GetByStatus(EntityStatus status, CancellationToken cancellationToken);

    Task<Vehicle?> GetById(Guid id, CancellationToken cancellationToken);
}
