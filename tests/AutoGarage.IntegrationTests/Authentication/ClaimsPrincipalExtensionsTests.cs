using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Auth.Exceptions;
using AutoGarage.Infrastructure.Authentication;
using Shouldly;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AutoGarage.IntegrationTests.Authentication;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetEmployeeId_WithSub_ShouldReturnIt()
    {
        var employeeId = Guid.NewGuid().ToString();
        var principal = Principal(new Claim(JwtRegisteredClaimNames.Sub, employeeId));

        principal.GetEmployeeId().ShouldBe(employeeId);
    }

    [Fact]
    public void GetEmployeeId_WithoutSub_ShouldThrowUnauthorized()
    {
        var principal = Principal(new Claim("role", "MECHANIC"));

        var exception = Should.Throw<MissingEmployeeIdentityException>(() => principal.GetEmployeeId());

        exception.ErrorType.ShouldBe("missing-employee-identity");
        exception.ShouldBeAssignableTo<UnauthorizedDomainException>();
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Bearer"));
}
