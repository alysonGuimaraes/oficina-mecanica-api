namespace AutoGarage.Domain.Exceptions.Services.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidServiceStatusException : BadRequestDomainException
{
    public InvalidServiceStatusException()
        : base("The provided status is invalid. Allowed values are ACTIVE or INACTIVE.", "invalid-service-status")
    {
    }
}
