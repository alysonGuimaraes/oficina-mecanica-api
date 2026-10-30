using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Communication.Responses.Vehicles;

namespace AutoGarage.Application.UseCases.Vehicles.TransferVehicleOwner;

public interface ITransferVehicleOwnerUseCase
{
    Task<VehicleResponse> Handle(TransferVehicleOwnerCommand command, CancellationToken cancellationToken);
}
