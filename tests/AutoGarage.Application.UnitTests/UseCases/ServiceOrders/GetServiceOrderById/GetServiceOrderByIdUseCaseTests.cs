using AutoGarage.Application.UseCases.ServiceOrders.GetServiceOrderById;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.GetServiceOrderById;

public class GetServiceOrderByIdUseCaseTests
{
    [Fact]
    public async Task Handle_ExistingOrder_ShouldReturnHeaderFields()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut(orders: [order]);

        var result = await sut.UseCase.Handle(order.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(order.Id);
        result.CustomerId.ShouldBe(order.CustomerId);
        result.VehicleId.ShouldBe(order.VehicleId);
        result.Status.ShouldBe(ServiceOrderStatus.RECEIVED);
        result.CreatedAt.ShouldBe(order.CreatedAt);
    }

    [Fact]
    public async Task Handle_NoBudgetSent_ShouldReturnNullBudgetTotalAndNoItems()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut(orders: [order]);

        var result = await sut.UseCase.Handle(order.Id.ToString(), CancellationToken.None);

        result.BudgetTotal.ShouldBeNull();
        result.Services.ShouldBeEmpty();
        result.Materials.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_OrderWithBudget_ShouldReturnBudgetTotal()
    {
        var employeeId = Guid.NewGuid();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, employeeId);
        order.SendBudget(150.00m, employeeId);
        var sut = CreateSut(orders: [order]);

        var result = await sut.UseCase.Handle(order.Id.ToString(), CancellationToken.None);

        result.BudgetTotal.ShouldBe(150.00m);
        result.BudgetSentAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ActiveItems_ShouldReturnServicesAndMaterialsWithTheirIds()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var service = ServiceOrderService.Create(order.Id, Guid.NewGuid(), 80m);
        var material = ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 2, 15m);
        var sut = CreateSut(orders: [order], services: [service], materials: [material]);

        var result = await sut.UseCase.Handle(order.Id.ToString(), CancellationToken.None);

        result.Services.Count.ShouldBe(1);
        result.Services[0].Id.ShouldBe(service.Id);
        result.Services[0].ServiceId.ShouldBe(service.ServiceId);
        result.Services[0].Price.ShouldBe(80m);

        result.Materials.Count.ShouldBe(1);
        result.Materials[0].Id.ShouldBe(material.Id);
        result.Materials[0].MaterialId.ShouldBe(material.MaterialId);
        result.Materials[0].Quantity.ShouldBe(2);
        result.Materials[0].UnitPrice.ShouldBe(15m);
    }

    [Fact]
    public async Task Handle_RemovedItem_ShouldNotBeInItemsButStaysInHistory()
    {
        var employeeId = Guid.NewGuid();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var service = ServiceOrderService.Create(order.Id, Guid.NewGuid(), 80m);
        service.Deactivate();
        var removal = ServiceOrderStatusHistory.ForItemChange(order.Id, ServiceOrderEventType.ITEM_SERVICE, service.Id, ServiceOrderItemDirection.OUT, employeeId);
        var sut = CreateSut(orders: [order], services: [service], histories: [removal]);

        var result = await sut.UseCase.Handle(order.Id.ToString(), CancellationToken.None);

        result.Services.ShouldBeEmpty();
        result.History.Count.ShouldBe(1);
        result.History[0].EventType.ShouldBe(ServiceOrderEventType.ITEM_SERVICE);
        result.History[0].ItemId.ShouldBe(service.Id);
    }

    [Fact]
    public async Task Handle_History_ShouldBeOrderedByCreatedAtThenId()
    {
        var employeeId = Guid.NewGuid();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var first = ServiceOrderStatusHistory.ForStatusChange(order.Id, null, ServiceOrderStatus.RECEIVED, employeeId);
        var second = ServiceOrderStatusHistory.ForStatusChange(order.Id, ServiceOrderStatus.RECEIVED, ServiceOrderStatus.IN_DIAGNOSIS, employeeId);
        var third = ServiceOrderStatusHistory.ForStatusChange(order.Id, ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.AWAITING_APPROVAL, employeeId);
        var sut = CreateSut(orders: [order], histories: [third, first, second]);

        var result = await sut.UseCase.Handle(order.Id.ToString(), CancellationToken.None);

        result.History.Count.ShouldBe(3);
        var expectedIds = new[] { first, second, third }
            .OrderBy(h => h.CreatedAt)
            .ThenBy(h => h.Id)
            .Select(h => h.Id)
            .ToList();
        result.History.Select(h => h.Id).ToList().ShouldBe(expectedIds);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    public async Task Handle_InvalidId_ShouldThrowInvalidServiceOrderIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.UseCase.Handle(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-id");
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();

        var exception = await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(id.ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-found");
    }

    private static (GetServiceOrderByIdUseCase UseCase, FakeServiceOrderRepository Orders) CreateSut(
        ServiceOrder[]? orders = null,
        ServiceOrderService[]? services = null,
        ServiceOrderMaterial[]? materials = null,
        ServiceOrderStatusHistory[]? histories = null)
    {
        var orderRepository = new FakeServiceOrderRepository();
        var serviceRepository = new FakeServiceOrderServiceRepository();
        var materialRepository = new FakeServiceOrderMaterialRepository();
        var historyRepository = new FakeServiceOrderStatusHistoryRepository();

        foreach (var order in orders ?? [])
        {
            orderRepository.Add(order, CancellationToken.None);
        }

        foreach (var service in services ?? [])
        {
            serviceRepository.Add(service, CancellationToken.None);
        }

        foreach (var material in materials ?? [])
        {
            materialRepository.Add(material, CancellationToken.None);
        }

        foreach (var history in histories ?? [])
        {
            historyRepository.Add(history, CancellationToken.None);
        }

        var useCase = new GetServiceOrderByIdUseCase(orderRepository, serviceRepository, materialRepository, historyRepository);

        return (useCase, orderRepository);
    }
}
