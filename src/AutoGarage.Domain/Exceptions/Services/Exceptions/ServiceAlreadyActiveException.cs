namespace AutoGarage.Domain.Exceptions.Services.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceAlreadyActiveException : ConflictDomainException
{
    public ServiceAlreadyActiveException(Guid id)
        : base($"Service with id '{id}' is already active.", "service-already-active")
    {
    }
}
