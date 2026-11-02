using AutoGarage.Application.UseCases.ServiceOrders.ListPublicServiceOrders;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.ListPublicServiceOrders;

public class ListPublicServiceOrdersUseCaseTests
{
    private const string ValidCpf = "39053344705";
    private const string OtherValidCpf = "12345678909";

    [Fact]
    public async Task Handle_KnownCustomer_ShouldReturnOrdersExceptDelivered()
    {
        var customer = CreateCustomer(ValidCpf);
        var received = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        var cancelled = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        cancelled.Cancel("motivo", Guid.NewGuid());
        var delivered = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        delivered.Cancel("motivo", Guid.NewGuid());
        delivered.ChangeStatus(ServiceOrderStatus.DELIVERED, Guid.NewGuid());
        var sut = CreateSut([customer], [received, cancelled, delivered]);

        var result = await sut.UseCase.Handle(ValidCpf, CancellationToken.None);

        result.Count.ShouldBe(2);
        var expectedIds = new[] { received, cancelled }
            .OrderBy(o => o.Status)
            .ThenBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Select(o => o.Id)
            .ToList();
        result.Select(o => o.Id).ToList().ShouldBe(expectedIds);
        result.Any(r => r.Status == ServiceOrderStatus.DELIVERED).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_DocumentWithMask_ShouldFindTheSameCustomer()
    {
        var customer = CreateCustomer(ValidCpf);
        var order = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        var sut = CreateSut([customer], [order]);

        var result = await sut.UseCase.Handle("390.533.447-05", CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(order.Id);
    }

    [Fact]
    public async Task Handle_UnknownDocument_ShouldReturnEmptyList()
    {
        var sut = CreateSut([], []);

        var result = await sut.UseCase.Handle(OtherValidCpf, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_KnownCustomerWithOnlyDeliveredOrders_ShouldReturnEmptyList()
    {
        var customer = CreateCustomer(ValidCpf);
        var delivered = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        delivered.Cancel("motivo", Guid.NewGuid());
        delivered.ChangeStatus(ServiceOrderStatus.DELIVERED, Guid.NewGuid());
        var sut = CreateSut([customer], [delivered]);

        var result = await sut.UseCase.Handle(ValidCpf, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_OtherCustomersOrders_ShouldNotBeReturned()
    {
        var customer = CreateCustomer(ValidCpf);
        var other = CreateCustomer(OtherValidCpf);
        var ownOrder = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        var otherOrder = ServiceOrder.Create(other.Id, Guid.NewGuid());
        var sut = CreateSut([customer, other], [ownOrder, otherOrder]);

        var result = await sut.UseCase.Handle(ValidCpf, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(ownOrder.Id);
    }

    [Fact]
    public async Task Handle_InvalidDocument_ShouldThrowInvalidDocumentException()
    {
        var sut = CreateSut([], []);

        var exception = await Should.ThrowAsync<InvalidDocumentException>(
            () => sut.UseCase.Handle("123", CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-document");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_BlankDocument_ShouldThrowErrorOnValidationException(string? document)
    {
        var sut = CreateSut([], []);

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(document, CancellationToken.None));
    }

    private static Customer CreateCustomer(string document)
    {
        return Customer.Create("John", new Document(document), $"{document}@autogarage.com", "41", "999999999");
    }

    private static ListPublicServiceOrdersUseCaseSut CreateSut(IEnumerable<Customer> customers, IEnumerable<ServiceOrder> orders)
    {
        var customerRepository = new FakeCustomerRepository();
        foreach (var customer in customers)
        {
            customerRepository.WithCustomer(customer);
        }

        var serviceOrderRepository = new FakeServiceOrderRepository();
        foreach (var order in orders)
        {
            serviceOrderRepository.Add(order, CancellationToken.None).Wait();
        }

        var useCase = new ListPublicServiceOrdersUseCase(customerRepository, serviceOrderRepository, MapperBuilder.Build());

        return new ListPublicServiceOrdersUseCaseSut(useCase);
    }

    private sealed record ListPublicServiceOrdersUseCaseSut(ListPublicServiceOrdersUseCase UseCase);
}
