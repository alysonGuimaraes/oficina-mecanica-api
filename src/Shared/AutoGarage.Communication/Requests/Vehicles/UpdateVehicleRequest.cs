namespace AutoGarage.Communication.Requests.Vehicles;

public record UpdateVehicleRequest(
    string Brand,
    string Model,
    string Plate,
    int Year
);
