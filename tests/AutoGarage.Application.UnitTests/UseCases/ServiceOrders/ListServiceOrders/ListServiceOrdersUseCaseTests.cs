using AutoGarage.Application.UseCases.ServiceOrders.ListServiceOrders;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.ListServiceOrders;

public class ListServiceOrdersUseCaseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NoStatus_ShouldReturnAllOrders(string? status)
    {
        var sut = CreateSut(
            CreateOrder(ServiceOrderStatus.RECEIVED),
            CreateOrder(ServiceOrderStatus.IN_DIAGNOSIS));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_StatusFilter_ShouldReturnOnlyMatchingOrders()
    {
        var sut = CreateSut(
            CreateOrder(ServiceOrderStatus.RECEIVED),
            CreateOrder(ServiceOrderStatus.IN_DIAGNOSIS));

        var result = await sut.UseCase.Handle("IN_DIAGNOSIS", CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Fact]
    public async Task Handle_LowercaseAndSpacedStatus_ShouldFilterNormally()
    {
        var sut = CreateSut(
            CreateOrder(ServiceOrderStatus.RECEIVED),
            CreateOrder(ServiceOrderStatus.IN_DIAGNOSIS));

        var result = await sut.UseCase.Handle(" in_diagnosis ", CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidServiceOrderStatusException()
    {
        var sut = CreateSut(CreateOrder(ServiceOrderStatus.RECEIVED));

        var exception = await Should.ThrowAsync<InvalidServiceOrderStatusException>(
            () => sut.UseCase.Handle("ENTREGUE", CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-status");
    }

    [Fact]
    public async Task Handle_NoOrders_ShouldReturnEmptyList()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_NoOrderWithStatus_ShouldReturnEmptyList()
    {
        var sut = CreateSut(CreateOrder(ServiceOrderStatus.RECEIVED));

        var result = await sut.UseCase.Handle("IN_DIAGNOSIS", CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static ServiceOrder CreateOrder(ServiceOrderStatus status)
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        if (status == ServiceOrderStatus.IN_DIAGNOSIS)
        {
            order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Guid.NewGuid());
        }

        return order;
    }

    private static (ListServiceOrdersUseCase UseCase, FakeServiceOrderRepository Repository) CreateSut(params ServiceOrder[] orders)
    {
        var repository = new FakeServiceOrderRepository();

        foreach (var order in orders)
        {
            repository.Add(order, CancellationToken.None);
        }

        var useCase = new ListServiceOrdersUseCase(repository, MapperBuilder.Build());

        return (useCase, repository);
    }
}
