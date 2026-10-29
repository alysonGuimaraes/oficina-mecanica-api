using AutoGarage.Domain.Entities;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Customers;

public class CustomerRepositoryExistsByEmailExcludingIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ExistsByEmailExcludingId_EmailOfAnotherCustomer_ShouldReturnTrue()
    {
        var customerA = CreateCustomer("52998224725", "a@autogarage.com");
        var customerB = CreateCustomer("12ABC34501DE35", "b@autogarage.com");
        await SaveAsync(customerA, customerB);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).ExistsByEmailExcludingId("B@AUTOGARAGE.COM", customerA.Id, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEmailExcludingId_OwnEmailInDifferentCase_ShouldReturnFalse()
    {
        var customer = CreateCustomer("52998224725", "john@autogarage.com");
        await SaveAsync(customer);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).ExistsByEmailExcludingId("JOHN@AUTOGARAGE.COM", customer.Id, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByEmailExcludingId_UnknownEmail_ShouldReturnFalse()
    {
        var customer = CreateCustomer("52998224725", "john@autogarage.com");
        await SaveAsync(customer);

        await using var context = CreateContext();
        var result = await new CustomerRepository(context).ExistsByEmailExcludingId("ninguem@autogarage.com", Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeFalse();
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
