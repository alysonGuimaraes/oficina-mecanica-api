namespace AutoGarage.Domain.Common.Exceptions;

public abstract class ConflictDomainException : DomainException
{
    protected ConflictDomainException(string message, string errorType) : base(message, errorType) { }
}
