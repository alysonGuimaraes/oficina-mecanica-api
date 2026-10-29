using AutoGarage.Application.UseCases.Customers.ListCustomers;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Customers.ListCustomers;

public class ListCustomersUseCaseTests
{
    [Fact]
    public async Task Handle_NoStatus_ShouldReturnOnlyActiveCustomers()
    {
        var sut = CreateSut(CreateCustomer("active@autogarage.com"), CreateCustomer("inactive@autogarage.com", inactive: true));

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyStatus_ShouldBeTreatedAsNotInformed(string status)
    {
        var sut = CreateSut(CreateCustomer("active@autogarage.com"), CreateCustomer("inactive@autogarage.com", inactive: true));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("Inactive")]
    public async Task Handle_InactiveStatus_ShouldReturnOnlyInactiveCustomers(string status)
    {
        var sut = CreateSut(CreateCustomer("active@autogarage.com"), CreateCustomer("inactive@autogarage.com", inactive: true));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_NoCustomersForStatus_ShouldReturnEmptyList()
    {
        var sut = CreateSut(CreateCustomer("active@autogarage.com"));

        var result = await sut.UseCase.Handle("INACTIVE", CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("FOO")]
    [InlineData("1")]
    [InlineData("0")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidCustomerStatusException(string status)
    {
        var sut = CreateSut(CreateCustomer("active@autogarage.com"));

        var exception = await Should.ThrowAsync<InvalidCustomerStatusException>(
            () => sut.UseCase.Handle(status, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-status");
        exception.Message.ShouldBe("The provided status is invalid. Allowed values are ACTIVE or INACTIVE.");
    }

    private static Customer CreateCustomer(string email, bool inactive = false)
    {
        var customer = Customer.Create("John", new Document("52998224725"), email, "41", "999999999");

        if (inactive)
        {
            customer.Deactivate();
        }

        return customer;
    }

    private static ListCustomersUseCaseSut CreateSut(params Customer[] customers)
    {
        var repository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            repository.WithCustomer(customer);
        }

        var useCase = new ListCustomersUseCase(repository, MapperBuilder.Build());

        return new ListCustomersUseCaseSut(useCase);
    }

    private sealed record ListCustomersUseCaseSut(ListCustomersUseCase UseCase);
}
