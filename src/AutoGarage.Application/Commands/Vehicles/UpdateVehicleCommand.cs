namespace AutoGarage.Application.Commands.Vehicles;

public record UpdateVehicleCommand(
    string Id,
    string Brand,
    string Model,
    string Plate,
    int Year,
    string CustomerId
);
