using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Customers;

public class CustomerRepositoryGetByDocumentTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetByDocument_SavedDocument_ShouldReturnCustomer()
    {
        var customer = CreateCustomer("12ABC34501DE35", "company@autogarage.com");
        await SaveAsync(customer);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetByDocument("12ABC34501DE35", CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(customer.Id);
    }

    [Fact]
    public async Task GetByDocument_InactiveCustomer_ShouldReturnItWithInactiveStatus()
    {
        var customer = CreateCustomer("52998224725", "inactive@autogarage.com");
        customer.Deactivate();
        await SaveAsync(customer);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetByDocument("52998224725", CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetByDocument_UnknownDocument_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var result = await new CustomerRepository(context).GetByDocument("11222333000181", CancellationToken.None);

        result.ShouldBeNull();
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
