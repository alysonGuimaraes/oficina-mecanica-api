using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Communication.Responses.Vehicles;

namespace AutoGarage.Application.UseCases.Vehicles.UpdateVehicle;

public interface IUpdateVehicleUseCase
{
    Task<VehicleResponse> Handle(UpdateVehicleCommand command, CancellationToken cancellationToken);
}
