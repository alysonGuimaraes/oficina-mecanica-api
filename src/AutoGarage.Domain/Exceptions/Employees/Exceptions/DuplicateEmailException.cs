namespace AutoGarage.Domain.Exceptions.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateEmailException : ConflictDomainException
{
    public DuplicateEmailException(string email)
        : base($"The email '{email}' is already registered in the system.", "duplicate-email")
    {
    }
}
