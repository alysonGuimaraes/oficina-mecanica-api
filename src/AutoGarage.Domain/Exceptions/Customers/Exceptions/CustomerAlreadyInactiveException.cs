namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class CustomerAlreadyInactiveException : ConflictDomainException
{
    public CustomerAlreadyInactiveException(Guid id)
        : base($"Customer with id '{id}' is already inactive.", "customer-already-inactive")
    {
    }
}
