using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Employees.UpdateEmployee;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Employees.UpdateEmployee;

public class UpdateEmployeeUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateFieldsAndReturnResponse()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);
        var command = new UpdateEmployeeCommand(employee.Id.ToString(), "  New   Name  ", "new@autogarage.com", EmployeeRole.ATTENDANT);

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Name.ShouldBe("New Name");
        result.Email.ShouldBe("NEW@AUTOGARAGE.COM");
        result.Role.ShouldBe(EmployeeRole.ATTENDANT);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldKeepPasswordHash()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var originalPasswordHash = employee.PasswordHash;
        var sut = CreateSut(employee);
        var command = new UpdateEmployeeCommand(employee.Id.ToString(), "John", "john.new@autogarage.com", EmployeeRole.MECHANIC);

        await sut.UseCase.Handle(command, CancellationToken.None);

        sut.Repository.Employees.Single().PasswordHash.ShouldBe(originalPasswordHash);
    }

    [Fact]
    public async Task Handle_SameEmailDifferentCase_ShouldNotThrowDuplicate()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);
        var command = new UpdateEmployeeCommand(employee.Id.ToString(), "John", "JOHN@AUTOGARAGE.COM", EmployeeRole.MECHANIC);

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Email.ShouldBe("JOHN@AUTOGARAGE.COM");
    }

    [Fact]
    public async Task Handle_EmailOfAnotherEmployee_ShouldThrowDuplicateEmailException()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var other = EmployeeBuilder.Build("taken@autogarage.com");
        var sut = CreateSut(employee, other);
        var command = new UpdateEmployeeCommand(employee.Id.ToString(), "John", "TAKEN@autogarage.com", EmployeeRole.MECHANIC);

        var exception = await Should.ThrowAsync<DuplicateEmailException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-email");
        sut.UnitWork.CommitCount.ShouldBe(0);
        sut.Repository.Employees.First(e => e.Id == employee.Id).Email.ShouldBe("john@autogarage.com");
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldUpdateAndKeepInactiveStatus()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        employee.Deactivate();
        var sut = CreateSut(employee);
        var command = new UpdateEmployeeCommand(employee.Id.ToString(), "John Updated", "john@autogarage.com", EmployeeRole.MECHANIC);

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Name.ShouldBe("John Updated");
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowEmployeeNotFoundException()
    {
        var sut = CreateSut();
        var command = new UpdateEmployeeCommand(Guid.NewGuid().ToString(), "John", "john@autogarage.com", EmployeeRole.MECHANIC);

        var exception = await Should.ThrowAsync<EmployeeNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-not-found");
    }

    [Fact]
    public async Task Handle_MalformedId_ShouldThrowInvalidEmployeeIdException()
    {
        var sut = CreateSut(EmployeeBuilder.Build("john@autogarage.com"));
        var command = new UpdateEmployeeCommand("abc", "John", "john@autogarage.com", EmployeeRole.MECHANIC);

        var exception = await Should.ThrowAsync<InvalidEmployeeIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-employee-id");
    }

    [Fact]
    public async Task Handle_InvalidBody_ShouldThrowErrorOnValidationException()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);
        var command = new UpdateEmployeeCommand(employee.Id.ToString(), "", "john@autogarage.com", EmployeeRole.MECHANIC);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The name field is required.");
    }

    private static UpdateEmployeeUseCaseSut CreateSut(params Employee[] employees)
    {
        var repository = new FakeEmployeeRepository();

        foreach (var employee in employees)
        {
            repository.WithEmployee(employee);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateEmployeeUseCase(
            repository,
            repository,
            unitWork,
            new StringNormalizer(),
            MapperBuilder.Build());

        return new UpdateEmployeeUseCaseSut(useCase, repository, unitWork);
    }

    private sealed record UpdateEmployeeUseCaseSut(
        UpdateEmployeeUseCase UseCase,
        FakeEmployeeRepository Repository,
        FakeUnitWork UnitWork);
}
