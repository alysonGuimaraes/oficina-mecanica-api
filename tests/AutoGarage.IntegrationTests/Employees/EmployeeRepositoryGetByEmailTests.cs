using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Employees;

public class EmployeeRepositoryGetByEmailTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetByEmail_ExactEmail_ShouldReturnEmployee()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        await SaveAsync(employee);

        await using var context = CreateContext();
        var result = await new EmployeeRepository(context).GetByEmail("john@autogarage.com", CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(employee.Id);
    }

    [Fact]
    public async Task GetByEmail_DifferentCase_ShouldReturnEmployee()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        await SaveAsync(employee);

        await using var context = CreateContext();
        var result = await new EmployeeRepository(context).GetByEmail("JOHN@AUTOGARAGE.COM", CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(employee.Id);
    }

    [Fact]
    public async Task GetByEmail_InactiveEmployee_ShouldReturnEmployeeWithInactiveStatus()
    {
        var employee = EmployeeBuilder.Build("inactive@autogarage.com");
        employee.Deactivate();
        await SaveAsync(employee);

        await using var context = CreateContext();
        var result = await new EmployeeRepository(context).GetByEmail("inactive@autogarage.com", CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetByEmail_UnknownEmail_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var result = await new EmployeeRepository(context).GetByEmail("nobody@autogarage.com", CancellationToken.None);

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
