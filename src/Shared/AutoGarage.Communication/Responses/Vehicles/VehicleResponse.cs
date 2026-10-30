using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Communication.Responses.Vehicles;

public record VehicleResponse(
    Guid Id,
    string Brand,
    string Model,
    string Plate,
    PlateType PlateType,
    int Year,
    VehicleType VehicleType,
    Guid CustomerId,
    EntityStatus Status
);
