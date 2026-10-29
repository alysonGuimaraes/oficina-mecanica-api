using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Customers;

public class CustomerRepositoryGetByIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetById_ExistingCustomer_ShouldReturnIt()
    {
        var customer = CreateCustomer("john@autogarage.com");
        await SaveAsync(customer);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetById(customer.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(customer.Id);
    }

    [Fact]
    public async Task GetById_InactiveCustomer_ShouldReturnItWithInactiveStatus()
    {
        var customer = CreateCustomer("inactive@autogarage.com");
        customer.Deactivate();
        await SaveAsync(customer);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetById(customer.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
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
