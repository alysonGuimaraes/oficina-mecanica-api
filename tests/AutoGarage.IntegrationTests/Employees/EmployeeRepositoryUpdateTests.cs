using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Employees;

public class EmployeeRepositoryUpdateTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ExistsByEmailExcludingId_SameEmployeeSameEmail_ShouldReturnFalse()
    {
        var employee = EmployeeBuilder.Build("john@autogarage.com");
        await SaveAsync(employee);

        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.ExistsByEmailExcludingId("john@autogarage.com", employee.Id, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByEmailExcludingId_OtherEmployeeDifferentCase_ShouldReturnTrue()
    {
        var first = EmployeeBuilder.Build("first@autogarage.com");
        var other = EmployeeBuilder.Build("other@autogarage.com");
        await SaveAsync(first, other);

        await using var context = CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.ExistsByEmailExcludingId("OTHER@autogarage.com", first.Id, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task Update_ChangedFields_ShouldPersistWithoutChangingPasswordHash()
    {
        var employee = EmployeeBuilder.Build("before@autogarage.com");
        await SaveAsync(employee);

        string originalPasswordHash;
        Employee loaded;

        await using (var readContext = CreateContext())
        {
            loaded = (await new EmployeeRepository(readContext).GetById(employee.Id, CancellationToken.None))!;
            originalPasswordHash = loaded.PasswordHash;
        }

        loaded.UpdateProfile("New Name", "new@autogarage.com", EmployeeRole.ATTENDANT);

        await using (var writeContext = CreateContext())
        {
            var repository = new EmployeeRepository(writeContext);
            await repository.Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Employees.AsNoTracking().SingleAsync(e => e.Id == employee.Id);

        persisted.Name.ShouldBe("New Name");
        persisted.Email.ShouldBe("new@autogarage.com");
        persisted.Role.ShouldBe(EmployeeRole.ATTENDANT);
        persisted.PasswordHash.ShouldBe(originalPasswordHash);
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
