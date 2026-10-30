using AutoGarage.Communication.Responses.Vehicles;

namespace AutoGarage.Application.UseCases.Vehicles.ListVehicles;

public interface IListVehiclesUseCase
{
    Task<IReadOnlyList<VehicleResponse>> Handle(string? status, CancellationToken cancellationToken);
}
