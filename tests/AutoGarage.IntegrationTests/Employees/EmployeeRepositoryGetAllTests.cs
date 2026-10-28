using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Employees;

public class EmployeeRepositoryGetAllTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetAll_EmptyTable_ShouldReturnEmptyList()
    {
        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetAll(CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_ActiveAndInactive_ShouldReturnBoth()
    {
        var active = EmployeeBuilder.Build("active@autogarage.com");
        var inactive = EmployeeBuilder.Build("inactive@autogarage.com");
        inactive.Deactivate();
        await SaveAsync(active, inactive);

        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetAll(CancellationToken.None);

        result.Count.ShouldBe(2);
        result.ShouldContain(employee => employee.Status == EntityStatus.ACTIVE);
        result.ShouldContain(employee => employee.Status == EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetAll_SameCreatedAt_ShouldOrderByIdAscending()
    {
        var first = EmployeeBuilder.Build("first@autogarage.com");
        var second = EmployeeBuilder.Build("second@autogarage.com");
        var sameCreatedAt = DateTime.UtcNow;

        await using (var setupContext = CreateContext())
        {
            setupContext.Employees.AddRange(first, second);
            setupContext.Entry(first).Property(nameof(Employee.CreatedAt)).CurrentValue = sameCreatedAt;
            setupContext.Entry(second).Property(nameof(Employee.CreatedAt)).CurrentValue = sameCreatedAt;
            await setupContext.SaveChangesAsync();
        }

        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetAll(CancellationToken.None);

        var expectedIds = new[] { first.Id, second.Id }.OrderBy(id => id).ToList();

        result.Select(employee => employee.Id).ShouldBe(expectedIds);
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
