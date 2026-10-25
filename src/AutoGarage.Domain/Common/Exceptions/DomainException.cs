namespace AutoGarage.Domain.Common.Exceptions;

public abstract class DomainException : Exception
{
    public string ErrorType { get; }

    protected DomainException(string message, string errorType) : base(message)
    {
        ErrorType = errorType;
    }
}
