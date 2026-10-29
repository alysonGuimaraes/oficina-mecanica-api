using AutoGarage.Application.Services.Cryptography;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.Cryptography;

public class PasswordEncripterVerifyTests
{
    private readonly PasswordEncripter _encripter = new("test-key");

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        var hash = _encripter.Encrypt("SecurePass123");

        _encripter.Verify("SecurePass123", hash).ShouldBeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ShouldReturnFalse()
    {
        var hash = _encripter.Encrypt("SecurePass123");

        _encripter.Verify("WrongPass123", hash).ShouldBeFalse();
    }

    [Fact]
    public void Verify_PasswordWithSurroundingSpaces_ShouldNotMatchTrimmedPassword()
    {
        var hash = _encripter.Encrypt("SecurePass123");

        _encripter.Verify("  SecurePass123  ", hash).ShouldBeFalse();
    }
}
