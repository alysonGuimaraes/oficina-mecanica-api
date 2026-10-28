namespace AutoGarage.Domain.Exceptions.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class EmployeeAlreadyActiveException : ConflictDomainException
{
    public EmployeeAlreadyActiveException(Guid id)
        : base($"Employee with id '{id}' is already active.", "employee-already-active")
    {
    }
}
