using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Communication.Responses.Vehicles;

namespace AutoGarage.Application.UseCases.Vehicles.CreateVehicle;

public interface ICreateVehicleUseCase
{
    Task<VehicleResponse> Handle(CreateVehicleCommand command, CancellationToken cancellationToken);
}
