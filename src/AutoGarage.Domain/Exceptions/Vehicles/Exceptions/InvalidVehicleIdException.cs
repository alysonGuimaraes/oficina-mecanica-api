namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidVehicleIdException : BadRequestDomainException
{
    public InvalidVehicleIdException()
        : base("The provided id is invalid.", "invalid-vehicle-id")
    {
    }
}
