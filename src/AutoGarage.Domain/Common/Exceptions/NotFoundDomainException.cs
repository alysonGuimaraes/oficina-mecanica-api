namespace AutoGarage.Domain.Common.Exceptions;

public abstract class NotFoundDomainException : DomainException
{
    protected NotFoundDomainException(string message, string errorType) : base(message, errorType) { }
}
