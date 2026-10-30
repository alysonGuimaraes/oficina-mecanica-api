namespace AutoGarage.Domain.Exceptions.Services.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceNotFoundException : NotFoundDomainException
{
    public ServiceNotFoundException(Guid id)
        : base($"Service with id '{id}' was not found.", "service-not-found")
    {
    }
}
