namespace AutoGarage.Domain.Exceptions.Services.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateServiceNameException : ConflictDomainException
{
    public DuplicateServiceNameException(string name)
        : base($"The service name '{name}' is already registered.", "duplicate-service-name")
    {
    }
}
