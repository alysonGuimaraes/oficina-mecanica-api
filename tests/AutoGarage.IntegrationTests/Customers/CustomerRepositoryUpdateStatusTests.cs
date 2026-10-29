using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Customers;

public class CustomerRepositoryUpdateStatusTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Update_Deactivated_ShouldPersistInactiveStatus()
    {
        var customer = CreateCustomer("john@autogarage.com");
        await SaveAsync(customer);

        Customer loaded;

        await using (var readContext = CreateContext())
        {
            loaded = (await new CustomerRepository(readContext).GetById(customer.Id, CancellationToken.None))!;
        }

        loaded.Deactivate();

        await using (var writeContext = CreateContext())
        {
            await new CustomerRepository(writeContext).Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Customers.AsNoTracking().SingleAsync(c => c.Id == customer.Id);

        persisted.Status.ShouldBe(EntityStatus.INACTIVE);
        persisted.Name.ShouldBe(customer.Name);
        persisted.Email.ShouldBe(customer.Email);
        persisted.Document.Value.ShouldBe("52998224725");
    }

    [Fact]
    public async Task Update_ReactivatedAfterDeactivation_ShouldPersistActiveStatus()
    {
        var customer = CreateCustomer("john@autogarage.com");
        customer.Deactivate();
        await SaveAsync(customer);

        Customer loaded;

        await using (var readContext = CreateContext())
        {
            loaded = (await new CustomerRepository(readContext).GetById(customer.Id, CancellationToken.None))!;
        }

        loaded.Activate();

        await using (var writeContext = CreateContext())
        {
            await new CustomerRepository(writeContext).Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Customers.AsNoTracking().SingleAsync(c => c.Id == customer.Id);

        persisted.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    private static Customer CreateCustomer(string email) =>
        Customer.Create("John", new Document("52998224725"), email, "41", "999999999");

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(params Customer[] customers)
    {
        await using var context = CreateContext();
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
    }
}
