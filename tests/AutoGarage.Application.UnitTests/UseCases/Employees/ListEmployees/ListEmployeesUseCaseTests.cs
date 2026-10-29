using AutoGarage.Application.UseCases.Employees.ListEmployees;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Employees.ListEmployees;

public class ListEmployeesUseCaseTests
{
    [Fact]
    public async Task Handle_EmployeesWithActiveAndInactive_ShouldReturnAllWithStatus()
    {
        var activeEmployee = EmployeeBuilder.Build("active@autogarage.com");
        var inactiveEmployee = EmployeeBuilder.Build("inactive@autogarage.com");
        inactiveEmployee.Deactivate();

        var sut = CreateSut(activeEmployee, inactiveEmployee);

        var result = await sut.UseCase.Handle(CancellationToken.None);

        result.Count.ShouldBe(2);
        result.ShouldContain(employee => employee.Status == EntityStatus.ACTIVE);
        result.ShouldContain(employee => employee.Status == EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_NoEmployees_ShouldReturnEmptyList()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static ListEmployeesUseCaseSut CreateSut(params AutoGarage.Domain.Entities.Employee[] employees)
    {
        var repository = new FakeEmployeeRepository();

        foreach (var employee in employees)
        {
            repository.WithEmployee(employee);
        }

        var useCase = new ListEmployeesUseCase(repository, MapperBuilder.Build());

        return new ListEmployeesUseCaseSut(useCase);
    }

    private sealed record ListEmployeesUseCaseSut(ListEmployeesUseCase UseCase);
}
