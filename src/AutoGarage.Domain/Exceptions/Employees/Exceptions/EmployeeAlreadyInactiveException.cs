namespace AutoGarage.Domain.Exceptions.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class EmployeeAlreadyInactiveException : ConflictDomainException
{
    public EmployeeAlreadyInactiveException(Guid id)
        : base($"Employee with id '{id}' is already inactive.", "employee-already-inactive")
    {
    }
}
