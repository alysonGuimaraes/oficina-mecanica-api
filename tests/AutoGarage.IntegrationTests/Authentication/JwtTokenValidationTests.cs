using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Infrastructure.Authentication;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutoGarage.IntegrationTests.Authentication;

public class JwtTokenValidationTests
{
    private const string Key = "unit-test-jwt-key-with-enough-length-0123456789";
    private const string Issuer = "autogarage";
    private const string Audience = "autogarage-api";

    [Fact]
    public void Validate_FreshToken_ShouldReturnSubjectAsEmployeeId()
    {
        var employee = Employee.Create("Ana", "ana@autogarage.local", "hash", EmployeeRole.MECHANIC);
        var token = Generate(employee, 15);

        var principal = Validate(token, JwtTokenValidation.Create(Settings(Key, Audience, 15)));

        principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.ShouldBe(employee.Id.ToString());
    }

    [Fact]
    public void Validate_ExpiredToken_ShouldBeRejected()
    {
        var employee = Employee.Create("Ana", "ana@autogarage.local", "hash", EmployeeRole.MECHANIC);
        var token = Generate(employee, -1);

        Should.Throw<SecurityTokenExpiredException>(
            () => Validate(token, JwtTokenValidation.Create(Settings(Key, Audience, 15))));
    }

    [Fact]
    public void Validate_WrongKey_ShouldBeRejected()
    {
        var employee = Employee.Create("Ana", "ana@autogarage.local", "hash", EmployeeRole.MECHANIC);
        var token = Generate(employee, 15);

        Should.Throw<SecurityTokenInvalidSignatureException>(
            () => Validate(token, JwtTokenValidation.Create(Settings("another-key-with-enough-length-0123456789", Audience, 15))));
    }

    [Fact]
    public void Validate_WrongAudience_ShouldBeRejected()
    {
        var employee = Employee.Create("Ana", "ana@autogarage.local", "hash", EmployeeRole.MECHANIC);
        var token = Generate(employee, 15);

        Should.Throw<SecurityTokenInvalidAudienceException>(
            () => Validate(token, JwtTokenValidation.Create(Settings(Key, "another-audience", 15))));
    }

    private static string Generate(Employee employee, int expiresInMinutes) =>
        new JwtAccessTokenGenerator(Settings(Key, Audience, expiresInMinutes)).Generate(employee).Token;

    private static ClaimsPrincipal Validate(string token, TokenValidationParameters parameters) =>
        new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token, parameters, out _);

    private static JwtSettings Settings(string key, string audience, int expiresInMinutes) => new()
    {
        Key = key,
        Issuer = Issuer,
        Audience = audience,
        ExpiresInMinutes = expiresInMinutes
    };
}
