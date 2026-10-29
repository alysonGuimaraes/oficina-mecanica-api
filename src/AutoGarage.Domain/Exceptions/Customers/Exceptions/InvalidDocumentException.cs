namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidDocumentException : BadRequestDomainException
{
    public InvalidDocumentException()
        : base("The provided document is invalid.", "invalid-document")
    {
    }
}
