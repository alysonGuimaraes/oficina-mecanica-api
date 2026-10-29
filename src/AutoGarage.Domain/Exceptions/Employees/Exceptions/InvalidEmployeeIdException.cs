namespace AutoGarage.Domain.Exceptions.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidEmployeeIdException : BadRequestDomainException
{
    public InvalidEmployeeIdException()
        : base("The provided id is invalid.", "invalid-employee-id")
    {
    }
}
