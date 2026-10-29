using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
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

    [Fact]
    public void Deactivate_ActiveCustomer_ShouldChangeStatusToInactiveAndSetUpdatedAt()
    {
        var customer = CreateCustomer();

        customer.Deactivate();

        customer.Status.ShouldBe(EntityStatus.INACTIVE);
        customer.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Activate_InactiveCustomer_ShouldChangeStatusToActive()
    {
        var customer = CreateCustomer();
        customer.Deactivate();

        customer.Activate();

        customer.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public void Activate_AlreadyActiveCustomer_ShouldThrowCustomerAlreadyActiveException()
    {
        var customer = CreateCustomer();

        var exception = Should.Throw<CustomerAlreadyActiveException>(() => customer.Activate());

        exception.ErrorType.ShouldBe("customer-already-active");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
    }

    [Fact]
    public void Deactivate_AlreadyInactiveCustomer_ShouldThrowCustomerAlreadyInactiveException()
    {
        var customer = CreateCustomer();
        customer.Deactivate();

        var exception = Should.Throw<CustomerAlreadyInactiveException>(() => customer.Deactivate());

        exception.ErrorType.ShouldBe("customer-already-inactive");
    }

    [Fact]
    public void UpdateProfile_ValidValues_ShouldChangeNameEmailDddPhoneAndSetUpdatedAt()
    {
        var customer = CreateCustomer();

        customer.UpdateProfile("Jane", "JANE@AUTOGARAGE.COM", "11", "888888888");

        customer.Name.ShouldBe("Jane");
        customer.Email.ShouldBe("JANE@AUTOGARAGE.COM");
        customer.Ddd.ShouldBe("11");
        customer.PhoneNumber.ShouldBe("888888888");
        customer.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateProfile_ValidValues_ShouldKeepDocumentStatusAndCreatedAt()
    {
        var customer = CreateCustomer();
        customer.Deactivate();
        var createdAt = customer.CreatedAt;

        customer.UpdateProfile("Jane", "JANE@AUTOGARAGE.COM", "11", "888888888");

        customer.Document.Value.ShouldBe("52998224725");
        customer.Status.ShouldBe(EntityStatus.INACTIVE);
        customer.CreatedAt.ShouldBe(createdAt);
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create("John", new Document("52998224725"), "john@autogarage.com", "41", "999999999");
    }
}
