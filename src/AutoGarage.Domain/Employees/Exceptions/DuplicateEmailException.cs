namespace AutoGarage.Domain.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateEmailException : DomainException
{
    public DuplicateEmailException(string email)
        : base($"The email '{email}' is already registered in the system.", "duplicate-email")
    {
    }
}
