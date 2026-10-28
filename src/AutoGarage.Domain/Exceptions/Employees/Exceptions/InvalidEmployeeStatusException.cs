namespace AutoGarage.Domain.Exceptions.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidEmployeeStatusException : BadRequestDomainException
{
    public InvalidEmployeeStatusException()
        : base("The provided status is invalid. Allowed values are ACTIVE or INACTIVE.", "invalid-employee-status")
    {
    }
}
