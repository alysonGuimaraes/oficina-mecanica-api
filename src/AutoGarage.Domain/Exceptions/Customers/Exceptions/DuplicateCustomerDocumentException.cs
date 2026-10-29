namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateCustomerDocumentException : ConflictDomainException
{
    public DuplicateCustomerDocumentException(string value)
        : base($"The document '{value}' is already registered for a customer.", "duplicate-customer-document")
    {
    }
}
