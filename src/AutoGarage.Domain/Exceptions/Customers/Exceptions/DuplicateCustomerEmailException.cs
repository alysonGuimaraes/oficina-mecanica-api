namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateCustomerEmailException : ConflictDomainException
{
    public DuplicateCustomerEmailException(string value)
        : base($"The email '{value}' is already registered for a customer.", "duplicate-customer-email")
    {
    }
}
