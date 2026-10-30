namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidVehicleStatusException : BadRequestDomainException
{
    public InvalidVehicleStatusException()
        : base("The provided status is invalid. Allowed values are ACTIVE or INACTIVE.", "invalid-vehicle-status")
    {
    }
}
