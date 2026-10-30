using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;

namespace AutoGarage.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public Plate Plate { get; private set; } = null!;
    public PlateType PlateType { get; private set; }
    public int Year { get; private set; }
    public VehicleType VehicleType { get; private set; }
    public Guid CustomerId { get; private set; }
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Vehicle() { }

    private Vehicle(string brand, string model, Plate plate, int year, VehicleType vehicleType, Guid customerId)
    {
        Id = Guid.NewGuid();
        Brand = brand;
        Model = model;
        Plate = plate;
        PlateType = plate.Type;
        Year = year;
        VehicleType = vehicleType;
        CustomerId = customerId;
        Status = EntityStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
    }

    public static Vehicle Create(string brand, string model, Plate plate, int year, VehicleType vehicleType, Guid customerId)
    {
        return new Vehicle(brand, model, plate, year, vehicleType, customerId);
    }

    public void Activate()
    {
        if (Status == EntityStatus.ACTIVE)
        {
            throw new VehicleAlreadyActiveException(Id);
        }

        Status = EntityStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.INACTIVE)
        {
            throw new VehicleAlreadyInactiveException(Id);
        }

        Status = EntityStatus.INACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }
}
