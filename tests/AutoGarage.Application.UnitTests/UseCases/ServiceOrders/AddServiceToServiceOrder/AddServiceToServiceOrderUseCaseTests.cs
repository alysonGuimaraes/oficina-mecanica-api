using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.AddServiceToServiceOrder;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.Services;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.AddServiceToServiceOrder;

public class AddServiceToServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_NewService_ShouldCreateActiveItemWithCatalogPriceAndInHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var service = Service.Create("Troca de óleo", null, 80m);
        var sut = CreateSut(orders: [order], employees: [employee], services: [service]);

        var result = await sut.UseCase.Handle(ValidCommand(order, service, employee), CancellationToken.None);

        sut.Items.Items.Count.ShouldBe(1);
        var item = sut.Items.Items[0];
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        item.Price.ShouldBe(80m);
        result.Id.ShouldBe(item.Id);
        result.ServiceId.ShouldBe(service.Id);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.EventType.ShouldBe(ServiceOrderEventType.ITEM_SERVICE);
        history.ItemId.ShouldBe(item.Id);
        history.ItemDirection.ShouldBe(ServiceOrderItemDirection.IN);
        history.FromStatus.ShouldBeNull();

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ServiceRemovedBefore_ShouldReactivateSameRowWithCurrentPrice()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var service = Service.Create("Troca de óleo", null, 95m);
        var existing = ServiceOrderService.Create(order.Id, service.Id, 80m);
        existing.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], services: [service], items: [existing]);

        var result = await sut.UseCase.Handle(ValidCommand(order, service, employee), CancellationToken.None);

        sut.Items.Items.Count.ShouldBe(1);
        sut.Items.Items[0].Id.ShouldBe(existing.Id);
        sut.Items.Items[0].Status.ShouldBe(EntityStatus.ACTIVE);
        sut.Items.Items[0].Price.ShouldBe(95m);
        result.Id.ShouldBe(existing.Id);

        sut.Histories.Histories.Count.ShouldBe(1);
        sut.Histories.Histories[0].ItemDirection.ShouldBe(ServiceOrderItemDirection.IN);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ServiceAlreadyActive_ShouldThrowServiceAlreadyAddedAndChangeNothing()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var service = Service.Create("Troca de óleo", null, 80m);
        var existing = ServiceOrderService.Create(order.Id, service.Id, 80m);
        var sut = CreateSut(orders: [order], employees: [employee], services: [service], items: [existing]);

        var exception = await Should.ThrowAsync<ServiceAlreadyAddedToServiceOrderException>(
            () => sut.UseCase.Handle(ValidCommand(order, service, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-already-added-to-service-order");
        sut.Items.Items.Count.ShouldBe(1);
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveCatalogService_ShouldThrowServiceOrderServiceInactive()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var service = Service.Create("Troca de óleo", null, 80m);
        service.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], services: [service]);

        var exception = await Should.ThrowAsync<ServiceOrderServiceInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, service, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-service-inactive");
        sut.Items.Items.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownService_ShouldThrowServiceNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new AddServiceToServiceOrderCommand(order.Id.ToString(), Guid.NewGuid().ToString(), employee.Id.ToString());

        var exception = await Should.ThrowAsync<ServiceNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("service-not-found");
    }

    [Fact]
    public async Task Handle_MalformedServiceId_ShouldThrowInvalidServiceIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new AddServiceToServiceOrderCommand(order.Id.ToString(), "abc", employee.Id.ToString());

        var exception = await Should.ThrowAsync<InvalidServiceIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-id");
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    public async Task Handle_OrderNotInDiagnosis_ShouldThrowServiceOrderNotEditableAndChangeNothing(ServiceOrderStatus status)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(status, employee.Id);
        var service = Service.Create("Troca de óleo", null, 80m);
        var sut = CreateSut(orders: [order], employees: [employee], services: [service]);

        var exception = await Should.ThrowAsync<ServiceOrderNotEditableException>(
            () => sut.UseCase.Handle(ValidCommand(order, service, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-editable");
        sut.Items.Items.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactive()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var service = Service.Create("Troca de óleo", null, 80m);
        var sut = CreateSut(orders: [order], employees: [employee], services: [service]);

        await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, service, employee), CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var service = Service.Create("Troca de óleo", null, 80m);
        var sut = CreateSut(employees: [employee], services: [service]);
        var command = new AddServiceToServiceOrderCommand(Guid.NewGuid().ToString(), service.Id.ToString(), employee.Id.ToString());

        await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));
    }

    private static AddServiceToServiceOrderCommand ValidCommand(ServiceOrder order, Service service, Employee employee) =>
        new(order.Id.ToString(), service.Id.ToString(), employee.Id.ToString());

    private static AddServiceToServiceOrderUseCaseSut CreateSut(
        ServiceOrder[]? orders = null,
        Employee[]? employees = null,
        Service[]? services = null,
        ServiceOrderService[]? items = null)
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

        var serviceRepository = new FakeServiceRepository();
        foreach (var service in services ?? [])
        {
            serviceRepository.WithService(service);
        }

        var itemRepository = new FakeServiceOrderServiceRepository();
        foreach (var item in items ?? [])
        {
            itemRepository.Add(item, CancellationToken.None);
        }

        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();
        var lookup = new ServiceOrderLookup(orderRepository, employeeRepository);

        var useCase = new AddServiceToServiceOrderUseCase(
            lookup, serviceRepository, itemRepository, itemRepository, historyRepository, unitWork);

        return new AddServiceToServiceOrderUseCaseSut(useCase, itemRepository, historyRepository, unitWork);
    }

    private sealed record AddServiceToServiceOrderUseCaseSut(
        AddServiceToServiceOrderUseCase UseCase,
        FakeServiceOrderServiceRepository Items,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
