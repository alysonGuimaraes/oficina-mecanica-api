using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.Infrastructure;
using AutoGarage.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Shouldly;

namespace AutoGarage.IntegrationTests.Authentication;

public class JwtAccessTokenGeneratorTests
{
    private const string Key = "test-signing-key-with-at-least-64-characters-for-hmac-sha256-0123456789";
    private const string Issuer = "autogarage";
    private const string Audience = "autogarage-api";

    [Fact]
    public void Generate_ShouldEmitTokenWithEmployeeClaims()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var generator = CreateGenerator();

        var (token, _) = generator.Generate(employee);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Single(c => c.Type == "sub").Value.ShouldBe(employee.Id.ToString());
        jwt.Claims.Single(c => c.Type == "email").Value.ShouldBe("john@autogarage.com");
        jwt.Claims.Single(c => c.Type == "role").Value.ShouldBe("MECHANIC");
        jwt.Claims.Any(c => c.Type == "jti").ShouldBeTrue();
        jwt.Issuer.ShouldBe(Issuer);
        jwt.Audiences.ShouldContain(Audience);
    }

    [Fact]
    public void Generate_ShouldSignWithConfiguredKey()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var (token, _) = CreateGenerator().Generate(employee);

        var validParameters = CreateValidationParameters(Key);
        Should.NotThrow(() => new JwtSecurityTokenHandler().ValidateToken(token, validParameters, out _));

        var otherKeyParameters = CreateValidationParameters("another-signing-key-with-at-least-64-characters-for-hmac-sha256-9876543210");
        Should.Throw<SecurityTokenInvalidSignatureException>(
            () => new JwtSecurityTokenHandler().ValidateToken(token, otherKeyParameters, out _));
    }

    [Fact]
    public void Generate_ExpiresAt_ShouldBeConfiguredMinutesFromNow()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");

        var (_, expiresAt) = CreateGenerator().Generate(employee);

        expiresAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(59));
        expiresAt.ShouldBeLessThan(DateTime.UtcNow.AddMinutes(61));
    }

    [Fact]
    public void AddInfrastructure_MissingKey_ShouldThrowInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=postgres;Password=x",
                ["Settings:Jwt:Issuer"] = Issuer,
                ["Settings:Jwt:Audience"] = Audience,
                ["Settings:Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        var exception = Should.Throw<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration));

        exception.Message.ShouldBe("Settings:Jwt:Key must be configured.");
    }

    private static JwtAccessTokenGenerator CreateGenerator() =>
        new(new JwtSettings
        {
            Key = Key,
            Issuer = Issuer,
            Audience = Audience,
            ExpiresInMinutes = 60
        });

    private static TokenValidationParameters CreateValidationParameters(string key) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
}
