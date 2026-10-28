namespace AutoGarage.Domain.Exceptions.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class EmployeeNotFoundException : NotFoundDomainException
{
    public EmployeeNotFoundException(Guid id)
        : base($"Employee with id '{id}' was not found.", "employee-not-found")
    {
    }
}
