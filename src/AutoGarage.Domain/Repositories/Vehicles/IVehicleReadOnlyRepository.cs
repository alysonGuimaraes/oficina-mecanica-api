namespace AutoGarage.Domain.Repositories.Vehicles;

public interface IVehicleReadOnlyRepository
{
    Task<bool> ExistsByPlate(string plate, CancellationToken cancellationToken);
}
