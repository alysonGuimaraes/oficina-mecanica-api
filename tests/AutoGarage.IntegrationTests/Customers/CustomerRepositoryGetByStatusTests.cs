using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Customers;

public class CustomerRepositoryGetByStatusTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetByStatus_Active_ShouldReturnOnlyActiveCustomers()
    {
        var inactive = CreateCustomer("12ABC34501DE35", "inactive@autogarage.com");
        inactive.Deactivate();
        await SaveAsync(CreateCustomer("52998224725", "active@autogarage.com"), inactive);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task GetByStatus_Inactive_ShouldReturnOnlyInactiveCustomers()
    {
        var inactive = CreateCustomer("12ABC34501DE35", "inactive@autogarage.com");
        inactive.Deactivate();
        await SaveAsync(CreateCustomer("52998224725", "active@autogarage.com"), inactive);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetByStatus(EntityStatus.INACTIVE, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetByStatus_OrdersByCreatedAtThenId()
    {
        var older = CreateCustomer("52998224725", "older@autogarage.com");
        var newer = CreateCustomer("11222333000181", "newer@autogarage.com");

        await using (var setupContext = CreateContext())
        {
            setupContext.Customers.AddRange(newer, older);
            setupContext.Entry(older).Property(nameof(Customer.CreatedAt)).CurrentValue = DateTime.UtcNow.AddDays(-1);
            setupContext.Entry(newer).Property(nameof(Customer.CreatedAt)).CurrentValue = DateTime.UtcNow;
            await setupContext.SaveChangesAsync();
        }

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Select(c => c.Id).ShouldBe([older.Id, newer.Id]);
    }

    [Fact]
    public async Task GetByStatus_NoMatches_ShouldReturnEmptyList()
    {
        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Customer CreateCustomer(string document, string email) =>
        Customer.Create("John", new Document(document), email, "41", "999999999");

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(params Customer[] customers)
    {
        await using var context = CreateContext();
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
    }
}
