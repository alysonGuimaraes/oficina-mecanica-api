namespace AutoGarage.Domain.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class EmployeeAlreadyInactiveException : DomainException
{
    public EmployeeAlreadyInactiveException(Guid id)
        : base($"Employee with id '{id}' is already inactive.", "employee-already-inactive")
    {
    }
}
