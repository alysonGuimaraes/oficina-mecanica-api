namespace AutoGarage.Domain.Common.Exceptions;

public class ErrorOnValidationException : AutoGarageException
{
    public IList<string> ErrorsMessages { get; set; }
    public ErrorOnValidationException(IList<string> errorMessages) : base(string.Empty)
    {
        ErrorsMessages = errorMessages;
    }
}
