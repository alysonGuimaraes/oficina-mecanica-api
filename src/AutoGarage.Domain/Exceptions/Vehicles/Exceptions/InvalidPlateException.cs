namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidPlateException : BadRequestDomainException
{
    public InvalidPlateException()
        : base("The provided plate is invalid.", "invalid-plate")
    {
    }
}
