using AutoGarage.Application.Commands.Auth;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Auth.Login;
using AutoGarage.CommonTestUtilities.Cryptography;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Services;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Exceptions.Auth.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Auth.Login;

public class LoginUseCaseTests
{
    private const string Password = "SecurePass123";

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnToken()
    {
        var employee = CreateEmployee("john@autogarage.com", Password);
        var sut = CreateSut(employee);

        var result = await sut.UseCase.Handle(new LoginCommand("john@autogarage.com", Password), CancellationToken.None);

        result.Token.ShouldBe($"token-{employee.Id}");
        result.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_EmailWithDifferentCaseAndSpaces_ShouldLogin()
    {
        var employee = CreateEmployee("john@autogarage.com", Password);
        var sut = CreateSut(employee);

        var result = await sut.UseCase.Handle(new LoginCommand("  JOHN@autogarage.com ", Password), CancellationToken.None);

        result.Token.ShouldBe($"token-{employee.Id}");
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldThrowInvalidCredentialsException()
    {
        var sut = CreateSut(CreateEmployee("john@autogarage.com", Password));

        var exception = await Should.ThrowAsync<InvalidCredentialsException>(
            () => sut.UseCase.Handle(new LoginCommand("john@autogarage.com", "WrongPass123"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-credentials");
        exception.Message.ShouldBe("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_UnknownEmail_ShouldThrowSameInvalidCredentialsException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidCredentialsException>(
            () => sut.UseCase.Handle(new LoginCommand("nobody@autogarage.com", Password), CancellationToken.None));

        exception.Message.ShouldBe("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_InactiveEmployeeWithCorrectPassword_ShouldThrowSameInvalidCredentialsException()
    {
        var employee = CreateEmployee("john@autogarage.com", Password);
        employee.Deactivate();
        var sut = CreateSut(employee);

        var exception = await Should.ThrowAsync<InvalidCredentialsException>(
            () => sut.UseCase.Handle(new LoginCommand("john@autogarage.com", Password), CancellationToken.None));

        exception.Message.ShouldBe("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_PasswordWithSurroundingSpaces_ShouldNotMatchTrimmedPassword()
    {
        var sut = CreateSut(CreateEmployee("john@autogarage.com", Password));

        await Should.ThrowAsync<InvalidCredentialsException>(
            () => sut.UseCase.Handle(new LoginCommand("john@autogarage.com", "  SecurePass123  "), CancellationToken.None));
    }

    [Theory]
    [InlineData("", "SecurePass123", "The email field is required.")]
    [InlineData("john@autogarage.com", "", "The password field is required.")]
    [InlineData("invalid-email", "SecurePass123", "The provided email format is invalid.")]
    public async Task Handle_InvalidBody_ShouldThrowErrorOnValidationException(string email, string password, string expectedMessage)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(new LoginCommand(email, password), CancellationToken.None));

        exception.ErrorsMessages.ShouldContain(expectedMessage);
    }

    private static Employee CreateEmployee(string email, string password)
    {
        var passwordHash = PasswordEncripterBuilder.Build().Encrypt(password);

        return Employee.Create("John", email, passwordHash, EmployeeRole.MECHANIC);
    }

    private static LoginUseCaseSut CreateSut(params Employee[] employees)
    {
        var repository = new FakeEmployeeRepository();

        foreach (var employee in employees)
        {
            repository.WithEmployee(employee);
        }

        var useCase = new LoginUseCase(
            repository,
            PasswordEncripterBuilder.Build(),
            new FakeAccessTokenGenerator(),
            new StringNormalizer());

        return new LoginUseCaseSut(useCase);
    }

    private sealed record LoginUseCaseSut(LoginUseCase UseCase);
}
