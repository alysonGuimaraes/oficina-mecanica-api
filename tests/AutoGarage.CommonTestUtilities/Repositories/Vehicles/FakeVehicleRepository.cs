using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Domain.Shared;

namespace AutoGarage.CommonTestUtilities.Repositories.Vehicles;

/// <summary>
/// Repositório em memória. Placa pelo valor normalizado, como no repositório real.
/// </summary>
public class FakeVehicleRepository : IVehicleReadOnlyRepository, IVehicleWriteOnlyRepository
{
    private readonly List<Vehicle> _vehicles = [];

    public IReadOnlyList<Vehicle> Vehicles => _vehicles;

    public FakeVehicleRepository WithVehicle(Vehicle vehicle)
    {
        _vehicles.Add(vehicle);
        return this;
    }

    public Task<bool> ExistsByPlate(string plate, CancellationToken cancellationToken)
    {
        var exists = _vehicles.Any(v => v.Plate.Value == plate);

        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Vehicle>> GetByStatus(EntityStatus status, CancellationToken cancellationToken)
    {
        IReadOnlyList<Vehicle> ordered = _vehicles
            .Where(v => v.Status == status)
            .OrderBy(v => v.CreatedAt)
            .ThenBy(v => v.Id)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task<Vehicle?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_vehicles.FirstOrDefault(v => v.Id == id));
    }

    public Task<bool> ExistsByPlateExcludingId(string plate, Guid id, CancellationToken cancellationToken)
    {
        var exists = _vehicles.Any(v => v.Id != id && v.Plate.Value == plate);

        return Task.FromResult(exists);
    }

    public Task Add(Vehicle vehicle, CancellationToken cancellationToken)
    {
        _vehicles.Add(vehicle);

        return Task.CompletedTask;
    }

    public Task Update(Vehicle vehicle, CancellationToken cancellationToken)
    {
        var index = _vehicles.FindIndex(v => v.Id == vehicle.Id);

        if (index >= 0)
        {
            _vehicles[index] = vehicle;
        }

        return Task.CompletedTask;
    }
}
