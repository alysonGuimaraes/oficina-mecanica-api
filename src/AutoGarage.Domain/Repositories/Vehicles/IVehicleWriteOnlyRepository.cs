using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.Vehicles;

public interface IVehicleWriteOnlyRepository
{
    Task Add(Vehicle vehicle, CancellationToken cancellationToken);
}
