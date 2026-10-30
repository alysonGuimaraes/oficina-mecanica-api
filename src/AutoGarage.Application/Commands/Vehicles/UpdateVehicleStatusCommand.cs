namespace AutoGarage.Application.Commands.Vehicles;

public record UpdateVehicleStatusCommand(
    string Id,
    string Status
);
