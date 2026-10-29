namespace AutoGarage.Domain.Common.Exceptions;

public abstract class BadRequestDomainException : DomainException
{
    protected BadRequestDomainException(string message, string errorType) : base(message, errorType) { }
}
