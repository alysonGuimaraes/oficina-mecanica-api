namespace AutoGarage.Domain.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class EmployeeNotFoundException : DomainException
{
    public EmployeeNotFoundException(Guid id)
        : base($"Employee with id '{id}' was not found.", "employee-not-found")
    {
    }
}
