using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.RemoveMaterialFromServiceOrder;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.RemoveMaterialFromServiceOrder;

public class RemoveMaterialFromServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_ActiveItem_ShouldDeactivateItemWithOutHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var item = ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 3, 2.50m);
        var sut = CreateSut(orders: [order], employees: [employee], items: [item]);

        var result = await sut.UseCase.Handle(ValidCommand(order, item, employee), CancellationToken.None);

        item.Status.ShouldBe(EntityStatus.INACTIVE);
        result.Id.ShouldBe(item.Id);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.EventType.ShouldBe(ServiceOrderEventType.ITEM_MATERIAL);
        history.ItemId.ShouldBe(item.Id);
        history.ItemDirection.ShouldBe(ServiceOrderItemDirection.OUT);

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ItemOfAnotherOrder_ShouldThrowServiceOrderItemNotFoundAndKeepActive()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var otherOrder = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var item = ServiceOrderMaterial.Create(otherOrder.Id, Guid.NewGuid(), 3, 2.50m);
        var sut = CreateSut(orders: [order, otherOrder], employees: [employee], items: [item]);

        var exception = await Should.ThrowAsync<ServiceOrderItemNotFoundException>(
            () => sut.UseCase.Handle(ValidCommand(order, item, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-item-not-found");
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownItem_ShouldThrowServiceOrderItemNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new RemoveMaterialFromServiceOrderCommand(order.Id.ToString(), Guid.NewGuid().ToString(), employee.Id.ToString());

        var exception = await Should.ThrowAsync<ServiceOrderItemNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-item-not-found");
    }

    [Fact]
    public async Task Handle_MalformedItemId_ShouldThrowInvalidServiceOrderItemIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new RemoveMaterialFromServiceOrderCommand(order.Id.ToString(), "abc", employee.Id.ToString());

        var exception = await Should.ThrowAsync<InvalidServiceOrderItemIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-item-id");
    }

    [Fact]
    public async Task Handle_AlreadyInactiveItem_ShouldThrowServiceOrderItemNotActiveAndChangeNothing()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var item = ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 3, 2.50m);
        item.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], items: [item]);

        var exception = await Should.ThrowAsync<ServiceOrderItemNotActiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, item, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-item-not-active");
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    public async Task Handle_OrderNotInDiagnosis_ShouldThrowServiceOrderNotEditableAndKeepActive(ServiceOrderStatus status)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(status, employee.Id);
        var item = ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 3, 2.50m);
        var sut = CreateSut(orders: [order], employees: [employee], items: [item]);

        var exception = await Should.ThrowAsync<ServiceOrderNotEditableException>(
            () => sut.UseCase.Handle(ValidCommand(order, item, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-editable");
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveAndKeepActive()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var item = ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 3, 2.50m);
        var sut = CreateSut(orders: [order], employees: [employee], items: [item]);

        await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, item, employee), CancellationToken.None));

        item.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new RemoveMaterialFromServiceOrderCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), employee.Id.ToString());

        await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));
    }

    private static RemoveMaterialFromServiceOrderCommand ValidCommand(ServiceOrder order, ServiceOrderMaterial item, Employee employee) =>
        new(order.Id.ToString(), item.Id.ToString(), employee.Id.ToString());

    private static RemoveMaterialFromServiceOrderUseCaseSut CreateSut(
        ServiceOrder[]? orders = null,
        Employee[]? employees = null,
        ServiceOrderMaterial[]? items = null)
    {
        var orderRepository = new FakeServiceOrderRepository();
        foreach (var order in orders ?? [])
        {
            orderRepository.Add(order, CancellationToken.None);
        }

        var employeeRepository = new FakeEmployeeRepository();
        foreach (var employee in employees ?? [])
        {
            employeeRepository.WithEmployee(employee);
        }

        var itemRepository = new FakeServiceOrderMaterialRepository();
        foreach (var item in items ?? [])
        {
            itemRepository.Add(item, CancellationToken.None);
        }

        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();
        var lookup = new ServiceOrderLookup(orderRepository, employeeRepository);

        var useCase = new RemoveMaterialFromServiceOrderUseCase(lookup, itemRepository, itemRepository, historyRepository, unitWork);

        return new RemoveMaterialFromServiceOrderUseCaseSut(useCase, historyRepository, unitWork);
    }

    private sealed record RemoveMaterialFromServiceOrderUseCaseSut(
        RemoveMaterialFromServiceOrderUseCase UseCase,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
