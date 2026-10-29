namespace AutoGarage.Domain.Common.Exceptions;

public abstract class UnauthorizedDomainException : DomainException
{
    protected UnauthorizedDomainException(string message, string errorType) : base(message, errorType) { }
}
