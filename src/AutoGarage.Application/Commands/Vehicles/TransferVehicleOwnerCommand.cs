namespace AutoGarage.Application.Commands.Vehicles;

public record TransferVehicleOwnerCommand(
    string Id,
    string NewOwnerDocument
);
