using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_CnpjDocument_ShouldCreateActiveCustomerWithCnpjType()
    {
        var document = new Document("11.222.333/0001-81");

        var customer = Customer.Create("John", document, "john@autogarage.com", "41", "999999999");

        customer.Status.ShouldBe(EntityStatus.ACTIVE);
        customer.DocumentType.ShouldBe(DocumentType.CNPJ);
        customer.Ddd.ShouldBe("41");
        customer.PhoneNumber.ShouldBe("999999999");
        customer.Id.ShouldNotBe(Guid.Empty);
        customer.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }
}
