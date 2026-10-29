namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class CustomerNotFoundException : NotFoundDomainException
{
    public CustomerNotFoundException(Guid id)
        : base($"Customer with id '{id}' was not found.", "customer-not-found")
    {
    }
}
