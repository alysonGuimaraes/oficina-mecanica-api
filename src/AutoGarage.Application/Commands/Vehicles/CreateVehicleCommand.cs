using AutoGarage.Domain.Enums.Vehicles;

namespace AutoGarage.Application.Commands.Vehicles;

public record CreateVehicleCommand(
    string Brand,
    string Model,
    string Plate,
    int Year,
    VehicleType VehicleType,
    string CustomerId
);
