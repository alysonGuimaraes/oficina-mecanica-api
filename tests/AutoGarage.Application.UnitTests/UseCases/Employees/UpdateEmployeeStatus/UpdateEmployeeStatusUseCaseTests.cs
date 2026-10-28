using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.UseCases.Employees.UpdateEmployeeStatus;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Employees.UpdateEmployeeStatus;

public class UpdateEmployeeStatusUseCaseTests
{
    [Fact]
    public async Task Handle_InactiveEmployeeToActive_ShouldActivate()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        employee.Deactivate();
        var sut = CreateSut(employee);

        var result = await sut.UseCase.Handle(new UpdateEmployeeStatusCommand(employee.Id.ToString(), "ACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ActiveEmployeeToInactive_ShouldDeactivate()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);

        var result = await sut.UseCase.Handle(new UpdateEmployeeStatusCommand(employee.Id.ToString(), "INACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("Inactive")]
    public async Task Handle_LowerOrMixedCaseStatus_ShouldBeAccepted(string status)
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);

        var result = await sut.UseCase.Handle(new UpdateEmployeeStatusCommand(employee.Id.ToString(), status), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_AlreadyActive_ShouldThrowEmployeeAlreadyActiveException()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);

        var exception = await Should.ThrowAsync<EmployeeAlreadyActiveException>(
            () => sut.UseCase.Handle(new UpdateEmployeeStatusCommand(employee.Id.ToString(), "ACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-already-active");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_AlreadyInactive_ShouldThrowEmployeeAlreadyInactiveException()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        employee.Deactivate();
        var sut = CreateSut(employee);

        var exception = await Should.ThrowAsync<EmployeeAlreadyInactiveException>(
            () => sut.UseCase.Handle(new UpdateEmployeeStatusCommand(employee.Id.ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-already-inactive");
    }

    [Theory]
    [InlineData("")]
    [InlineData("FOO")]
    [InlineData("PENDING")]
    [InlineData("1")]
    [InlineData("0")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidEmployeeStatusException(string status)
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);

        var exception = await Should.ThrowAsync<InvalidEmployeeStatusException>(
            () => sut.UseCase.Handle(new UpdateEmployeeStatusCommand(employee.Id.ToString(), status), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-employee-status");
        exception.Message.ShouldBe("The provided status is invalid. Allowed values are ACTIVE or INACTIVE.");
        sut.UnitWork.CommitCount.ShouldBe(0);
        sut.Repository.Employees.Single().Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowEmployeeNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<EmployeeNotFoundException>(
            () => sut.UseCase.Handle(new UpdateEmployeeStatusCommand(Guid.NewGuid().ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-not-found");
    }

    [Fact]
    public async Task Handle_MalformedId_ShouldThrowInvalidEmployeeIdException()
    {
        var sut = CreateSut(EmployeeBuilder.Build("john@autogarage.com"));

        var exception = await Should.ThrowAsync<InvalidEmployeeIdException>(
            () => sut.UseCase.Handle(new UpdateEmployeeStatusCommand("abc", "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-employee-id");
    }

    [Fact]
    public async Task Handle_StatusChange_ShouldKeepOtherFieldsAndPasswordHash()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var (name, email, role, passwordHash) = (employee.Name, employee.Email, employee.Role, employee.PasswordHash);
        var sut = CreateSut(employee);

        await sut.UseCase.Handle(new UpdateEmployeeStatusCommand(employee.Id.ToString(), "INACTIVE"), CancellationToken.None);

        var stored = sut.Repository.Employees.Single();
        stored.Name.ShouldBe(name);
        stored.Email.ShouldBe(email);
        stored.Role.ShouldBe(role);
        stored.PasswordHash.ShouldBe(passwordHash);
    }

    private static UpdateEmployeeStatusUseCaseSut CreateSut(params Employee[] employees)
    {
        var repository = new FakeEmployeeRepository();

        foreach (var employee in employees)
        {
            repository.WithEmployee(employee);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateEmployeeStatusUseCase(repository, repository, unitWork, MapperBuilder.Build());

        return new UpdateEmployeeStatusUseCaseSut(useCase, repository, unitWork);
    }

    private sealed record UpdateEmployeeStatusUseCaseSut(
        UpdateEmployeeStatusUseCase UseCase,
        FakeEmployeeRepository Repository,
        FakeUnitWork UnitWork);
}
