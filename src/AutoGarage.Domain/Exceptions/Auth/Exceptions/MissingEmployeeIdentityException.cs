using AutoGarage.Domain.Common.Exceptions;

namespace AutoGarage.Domain.Exceptions.Auth.Exceptions;

public class MissingEmployeeIdentityException : UnauthorizedDomainException
{
    public MissingEmployeeIdentityException()
        : base("The access token does not identify an employee.", "missing-employee-identity")
    {
    }
}
