namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class CustomerAlreadyActiveException : ConflictDomainException
{
    public CustomerAlreadyActiveException(Guid id)
        : base($"Customer with id '{id}' is already active.", "customer-already-active")
    {
    }
}
