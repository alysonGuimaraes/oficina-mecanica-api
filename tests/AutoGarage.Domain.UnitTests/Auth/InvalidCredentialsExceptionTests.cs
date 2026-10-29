using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Auth.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Auth;

public class InvalidCredentialsExceptionTests
{
    [Fact]
    public void Constructor_ShouldUseInvalidCredentialsErrorTypeAndGenericMessage()
    {
        var exception = new InvalidCredentialsException();

        exception.ErrorType.ShouldBe("invalid-credentials");
        exception.Message.ShouldBe("Invalid email or password.");
        exception.ShouldBeAssignableTo<UnauthorizedDomainException>();
    }
}
