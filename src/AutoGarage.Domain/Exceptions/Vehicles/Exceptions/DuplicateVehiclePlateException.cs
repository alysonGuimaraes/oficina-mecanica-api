namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateVehiclePlateException : ConflictDomainException
{
    public DuplicateVehiclePlateException(string plate)
        : base($"The plate '{plate}' is already registered for a vehicle.", "duplicate-vehicle-plate")
    {
    }
}
