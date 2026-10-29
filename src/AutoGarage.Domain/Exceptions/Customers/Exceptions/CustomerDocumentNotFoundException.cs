namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common;
using AutoGarage.Domain.Common.Exceptions;

public class CustomerDocumentNotFoundException : NotFoundDomainException
{
    public CustomerDocumentNotFoundException(string document)
        : base($"Customer with document '{DocumentMask.Mask(document)}' was not found.", "customer-document-not-found")
    {
    }
}
