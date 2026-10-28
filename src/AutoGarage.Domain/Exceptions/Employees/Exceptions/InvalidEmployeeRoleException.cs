namespace AutoGarage.Domain.Exceptions.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidEmployeeRoleException : BadRequestDomainException
{
    public InvalidEmployeeRoleException(string role)
        : base($"The role '{role}' is invalid. Allowed roles are MECHANIC or ATTENDANT.", "invalid-employee-role")
    {
    }
}
