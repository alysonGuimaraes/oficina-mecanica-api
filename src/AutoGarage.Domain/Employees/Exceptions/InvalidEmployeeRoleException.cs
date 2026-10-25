namespace AutoGarage.Domain.Employees.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidEmployeeRoleException : DomainException
{
    public InvalidEmployeeRoleException(string role)
        : base($"The role '{role}' is invalid. Allowed roles are Mechanic or Attendant.", "invalid-employee-role")
    {
    }
}
