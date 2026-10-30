using AutoGarage.Communication.Responses.Vehicles;

namespace AutoGarage.Application.UseCases.Vehicles.GetVehicleById;

public interface IGetVehicleByIdUseCase
{
    Task<VehicleResponse> Handle(string id, CancellationToken cancellationToken);
}
