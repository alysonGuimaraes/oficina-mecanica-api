using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Vehicles;

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

    public Task Add(Vehicle vehicle, CancellationToken cancellationToken)
    {
        _vehicles.Add(vehicle);

        return Task.CompletedTask;
    }
}
