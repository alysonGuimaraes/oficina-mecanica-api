using AutoGarage.Application.UseCases.Employees.GetEmployeeById;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Employees.GetEmployeeById;

public class GetEmployeeByIdUseCaseTests
{
    [Fact]
    public async Task Handle_ExistingEmployee_ShouldReturnResponse()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        var sut = CreateSut(employee);

        var result = await sut.UseCase.Handle(employee.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(employee.Id);
        result.Email.ShouldBe("john@autogarage.com");
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldReturnWithInactiveStatus()
    {
        var employee = EmployeeBuilder.Build("inactive@autogarage.com");
        employee.Deactivate();
        var sut = CreateSut(employee);

        var result = await sut.UseCase.Handle(employee.Id.ToString(), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowEmployeeNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<EmployeeNotFoundException>(
            () => sut.UseCase.Handle(Guid.NewGuid().ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-not-found");
    }

    [Fact]
    public async Task Handle_EmptyGuid_ShouldThrowEmployeeNotFoundException()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<EmployeeNotFoundException>(
            () => sut.UseCase.Handle(Guid.Empty.ToString(), CancellationToken.None));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("12345")]
    public async Task Handle_MalformedId_ShouldThrowInvalidEmployeeIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidEmployeeIdException>(
            () => sut.UseCase.Handle(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-employee-id");
        exception.Message.ShouldBe("The provided id is invalid.");
        exception.ShouldBeOfType<InvalidEmployeeIdException>();
    }

    private static GetEmployeeByIdUseCaseSut CreateSut(params Employee[] employees)
    {
        var repository = new FakeEmployeeRepository();

        foreach (var employee in employees)
        {
            repository.WithEmployee(employee);
        }

        var useCase = new GetEmployeeByIdUseCase(repository, MapperBuilder.Build());

        return new GetEmployeeByIdUseCaseSut(useCase);
    }

    private sealed record GetEmployeeByIdUseCaseSut(GetEmployeeByIdUseCase UseCase);
}
