using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Communication.Responses.Vehicles;

namespace AutoGarage.Application.UseCases.Vehicles.UpdateVehicleStatus;

public interface IUpdateVehicleStatusUseCase
{
    Task<VehicleResponse> Handle(UpdateVehicleStatusCommand command, CancellationToken cancellationToken);
}
