namespace AutoGarage.Domain.Exceptions.Services.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidServiceIdException : BadRequestDomainException
{
    public InvalidServiceIdException()
        : base("The provided id is invalid.", "invalid-service-id")
    {
    }
}
