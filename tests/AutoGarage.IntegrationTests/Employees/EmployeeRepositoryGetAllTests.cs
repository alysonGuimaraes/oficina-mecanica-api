using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using AutoGarage.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Employees;

public class EmployeeRepositoryGetAllTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public EmployeeRepositoryGetAllTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAll_EmptyTable_ShouldReturnEmptyList()
    {
        await using var context = await CreateCleanContextAsync();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetAll(CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_ActiveAndInactive_ShouldReturnBoth()
    {
        await using var context = await CreateCleanContextAsync();
        var active = EmployeeBuilder.Build("active@autogarage.com");
        var inactive = EmployeeBuilder.Build("inactive@autogarage.com");
        inactive.Deactivate();
        context.Employees.AddRange(active, inactive);
        await context.SaveChangesAsync();

        await using var readContext = AppDbContextFactory();
        var repository = new EmployeeRepository(readContext);

        var result = await repository.GetAll(CancellationToken.None);

        result.Count.ShouldBe(2);
        result.ShouldContain(employee => employee.Status == AutoGarage.Domain.Shared.EntityStatus.ACTIVE);
        result.ShouldContain(employee => employee.Status == AutoGarage.Domain.Shared.EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetAll_SameCreatedAt_ShouldOrderByIdAscending()
    {
        await using var context = await CreateCleanContextAsync();
        var first = EmployeeBuilder.Build("first@autogarage.com");
        var second = EmployeeBuilder.Build("second@autogarage.com");
        context.Employees.AddRange(first, second);
        await context.SaveChangesAsync();

        var sameCreatedAt = DateTime.UtcNow;
        await context.Database.ExecuteSqlAsync($"UPDATE employees SET \"CreatedAt\" = {sameCreatedAt}");

        // A ordem esperada vem do próprio PostgreSQL: uuid ordena por bytes, diferente de Guid.CompareTo.
        var expectedIds = await context.Database
            .SqlQuery<Guid>($"SELECT \"Id\" FROM employees ORDER BY \"Id\"")
            .ToListAsync();

        await using var readContext = AppDbContextFactory();
        var repository = new EmployeeRepository(readContext);

        var result = await repository.GetAll(CancellationToken.None);

        result.Select(employee => employee.Id).ShouldBe(expectedIds);
    }

    private async Task<AppDbContext> CreateCleanContextAsync()
    {
        var context = AppDbContextFactory();
        await context.Database.ExecuteSqlRawAsync("DELETE FROM employees");
        return context;
    }

    private AppDbContext AppDbContextFactory() => PostgresFixture.CreateContext(_fixture.ConnectionString);
}
