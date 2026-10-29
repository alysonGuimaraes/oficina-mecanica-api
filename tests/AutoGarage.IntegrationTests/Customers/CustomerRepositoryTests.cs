using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Customers;

public class CustomerRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ExistsByDocument_SavedDocument_ShouldReturnTrue()
    {
        await SaveAsync(CreateCustomer("12ABC34501DE35", "john@autogarage.com"));

        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        var result = await repository.ExistsByDocument("12ABC34501DE35", CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByDocument_UnknownDocument_ShouldReturnFalse()
    {
        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        var result = await repository.ExistsByDocument("52998224725", CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByEmail_DifferentCase_ShouldReturnTrue()
    {
        await SaveAsync(CreateCustomer("52998224725", "JOHN@AUTOGARAGE.COM"));

        await using var context = CreateContext();
        var repository = new CustomerRepository(context);

        var result = await repository.ExistsByEmail("john@autogarage.com", CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task Add_ValidCustomer_ShouldPersistStatusDocumentTypeAndPhoneParts()
    {
        var customer = CreateCustomer("11222333000181", "company@autogarage.com");

        await using (var writeContext = CreateContext())
        {
            await new CustomerRepository(writeContext).Add(customer, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Customers.AsNoTracking().SingleAsync(c => c.Id == customer.Id);

        persisted.Status.ShouldBe(EntityStatus.ACTIVE);
        persisted.DocumentType.ShouldBe(DocumentType.CNPJ);
        persisted.Document.Value.ShouldBe("11222333000181");
        persisted.Ddd.ShouldBe("41");
        persisted.PhoneNumber.ShouldBe("999999999");
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
