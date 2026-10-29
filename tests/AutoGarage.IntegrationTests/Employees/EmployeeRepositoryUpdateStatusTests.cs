using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Employees;

public class EmployeeRepositoryUpdateStatusTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Update_Deactivated_ShouldPersistInactiveStatus()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        await SaveAsync(employee);

        Employee loaded;
        string originalPasswordHash;

        await using (var readContext = CreateContext())
        {
            loaded = (await new EmployeeRepository(readContext).GetById(employee.Id, CancellationToken.None))!;
            originalPasswordHash = loaded.PasswordHash;
        }

        loaded.Deactivate();

        await using (var writeContext = CreateContext())
        {
            await new EmployeeRepository(writeContext).Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Employees.AsNoTracking().SingleAsync(e => e.Id == employee.Id);

        persisted.Status.ShouldBe(EntityStatus.INACTIVE);
        persisted.Name.ShouldBe(employee.Name);
        persisted.Email.ShouldBe(employee.Email);
        persisted.PasswordHash.ShouldBe(originalPasswordHash);
    }

    [Fact]
    public async Task Update_ReactivatedAfterDeactivation_ShouldPersistActiveStatus()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        employee.Deactivate();
        await SaveAsync(employee);

        Employee loaded;

        await using (var readContext = CreateContext())
        {
            loaded = (await new EmployeeRepository(readContext).GetById(employee.Id, CancellationToken.None))!;
        }

        loaded.Activate();

        await using (var writeContext = CreateContext())
        {
            await new EmployeeRepository(writeContext).Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Employees.AsNoTracking().SingleAsync(e => e.Id == employee.Id);

        persisted.Status.ShouldBe(EntityStatus.ACTIVE);
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
