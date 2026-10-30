using AutoGarage.Domain.Enums.Vehicles;

namespace AutoGarage.Communication.Requests.Vehicles;

public record CreateVehicleRequest(
    string Brand,
    string Model,
    string Plate,
    int Year,
    VehicleType VehicleType,
    string CustomerId
);
