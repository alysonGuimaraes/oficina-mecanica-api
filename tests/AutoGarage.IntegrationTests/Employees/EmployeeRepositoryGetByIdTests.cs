using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Employees;

public class EmployeeRepositoryGetByIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetById_ExistingEmployee_ShouldReturnIt()
    {
        var employee = EmployeeBuilder.Build("existing@autogarage.com");
        await SaveAsync(employee);

        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetById(employee.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(employee.Id);
    }

    [Fact]
    public async Task GetById_InactiveEmployee_ShouldReturnIt()
    {
        var employee = EmployeeBuilder.Build("inactive@autogarage.com");
        employee.Deactivate();
        await SaveAsync(employee);

        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetById(employee.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(params Employee[] employees)
    {
        await using var context = CreateContext();
        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();
    }
}
