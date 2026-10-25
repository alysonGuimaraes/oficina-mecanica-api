namespace AutoGarage.Domain.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class EmployeeAlreadyActiveException : DomainException
{
    public EmployeeAlreadyActiveException(Guid id)
        : base($"Employee with id '{id}' is already active.", "employee-already-active")
    {
    }
}
