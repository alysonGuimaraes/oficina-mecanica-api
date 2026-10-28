using AutoGarage.Application.UseCases.Employees.CreateEmployee;
using AutoGarage.CommonTestUtilities.Commands.Employees;
using AutoGarage.CommonTestUtilities.Cryptography;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.UnitTests.Application.UseCases.Employees.CreateEmployee;

public class CreateEmployeeUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateActiveEmployeeAndReturnResponse()
    {
        var command = new CreateEmployeeCommandBuilder().Build();
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);
        result.Email.ShouldBe(command.Email.ToUpperInvariant());
        result.Role.ShouldBe(command.Role);
        result.Status.ShouldBe(EntityStatus.ACTIVE);

        sut.Repository.Employees.ShouldHaveSingleItem();
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldStoreHashedPassword()
    {
        var command = new CreateEmployeeCommandBuilder().WithPassword("SecurePass123").Build();
        var sut = CreateSut();

        await sut.UseCase.Handle(command, CancellationToken.None);

        var stored = sut.Repository.Employees.Single();

        stored.PasswordHash.ShouldNotBe(command.Password);
        stored.PasswordHash.ShouldBe(PasswordEncripterBuilder.Build().Encrypt(command.Password));
    }

    [Theory]
    [InlineData("", "john@autogarage.com", "SecurePass123", "The name field is required.")]
    [InlineData("John", "invalid-email", "SecurePass123", "The provided email format is invalid.")]
    [InlineData("John", "john@autogarage.com", "12345", "The password must be at least 6 characters long.")]
    public async Task Handle_InvalidCommand_ShouldThrowValidationExceptionWithMessage(
        string name,
        string email,
        string password,
        string expectedMessage)
    {
        var command = new CreateEmployeeCommandBuilder()
            .WithName(name)
            .WithEmail(email)
            .WithPassword(password)
            .Build();
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorsMessages.ShouldContain(expectedMessage);
        sut.Repository.Employees.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidRole_ShouldThrowValidationException()
    {
        var command = new CreateEmployeeCommandBuilder().WithRole((EmployeeRole)99).Build();
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The provided role is invalid. Allowed roles are MECHANIC or ATTENDANT.");
        sut.Repository.Employees.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldThrowDuplicateEmailException()
    {
        var command = new CreateEmployeeCommandBuilder().WithEmail("existing@autogarage.com").Build();
        var sut = CreateSut(existingEmployeeEmail: command.Email);

        var exception = await Should.ThrowAsync<DuplicateEmailException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-email");
        sut.Repository.Employees.Count.ShouldBe(1);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_DuplicateEmailWithDifferentCase_ShouldThrowDuplicateEmailException()
    {
        var command = new CreateEmployeeCommandBuilder().WithEmail("john@autogarage.com").Build();
        var sut = CreateSut(existingEmployeeEmail: "JOHN@autogarage.com");

        await Should.ThrowAsync<DuplicateEmailException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_NameAndEmailWithSurroundingSpaces_ShouldStoreTrimmedValues()
    {
        var command = new CreateEmployeeCommandBuilder()
            .WithName("  John Mechanic  ")
            .WithEmail("  john.mechanic@autogarage.com ")
            .Build();
        var sut = CreateSut();

        await sut.UseCase.Handle(command, CancellationToken.None);

        var stored = sut.Repository.Employees.Single();

        stored.Name.ShouldBe("John Mechanic");
        stored.Email.ShouldBe("JOHN.MECHANIC@AUTOGARAGE.COM");
    }

    [Fact]
    public async Task Handle_NameWithExtraInnerSpaces_ShouldCollapseToSingleSpace()
    {
        var command = new CreateEmployeeCommandBuilder().WithName("John    Mechanic   Jr").Build();
        var sut = CreateSut();

        await sut.UseCase.Handle(command, CancellationToken.None);

        sut.Repository.Employees.Single().Name.ShouldBe("John Mechanic Jr");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldStoreEmailInUpperCase()
    {
        var command = new CreateEmployeeCommandBuilder().WithEmail("john.mechanic@autogarage.com").Build();
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Email.ShouldBe("JOHN.MECHANIC@AUTOGARAGE.COM");
    }

    [Fact]
    public async Task Handle_PasswordWithSurroundingSpaces_ShouldKeepPasswordUntouched()
    {
        var command = new CreateEmployeeCommandBuilder().WithPassword("  SecurePass123  ").Build();
        var sut = CreateSut();

        await sut.UseCase.Handle(command, CancellationToken.None);

        sut.Repository.Employees.Single().PasswordHash
            .ShouldBe(PasswordEncripterBuilder.Build().Encrypt("  SecurePass123  "));
    }

    private static CreateEmployeeUseCaseSut CreateSut(string? existingEmployeeEmail = null)
    {
        var repository = new FakeEmployeeRepository();

        if (existingEmployeeEmail is not null)
        {
            repository.WithEmployee(EmployeeBuilder.Build(existingEmployeeEmail));
        }

        var unitWork = new FakeUnitWork();

        var useCase = new CreateEmployeeUseCase(
            repository,
            repository,
            unitWork,
            PasswordEncripterBuilder.Build(),
            MapperBuilder.Build());

        return new CreateEmployeeUseCaseSut(useCase, repository, unitWork);
    }

    private sealed record CreateEmployeeUseCaseSut(
        CreateEmployeeUseCase UseCase,
        FakeEmployeeRepository Repository,
        FakeUnitWork UnitWork);
}
