namespace AutoGarage.Domain.Exceptions.Services.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceAlreadyInactiveException : ConflictDomainException
{
    public ServiceAlreadyInactiveException(Guid id)
        : base($"Service with id '{id}' is already inactive.", "service-already-inactive")
    {
    }
}
