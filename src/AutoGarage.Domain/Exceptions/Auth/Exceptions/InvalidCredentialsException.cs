namespace AutoGarage.Domain.Exceptions.Auth.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidCredentialsException : UnauthorizedDomainException
{
    public InvalidCredentialsException()
        : base("Invalid email or password.", "invalid-credentials")
    {
    }
}
