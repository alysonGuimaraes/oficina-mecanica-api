using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.AddMaterialToServiceOrder;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.AddMaterialToServiceOrder;

public class AddMaterialToServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_NewMaterial_ShouldCreateActiveItemWithCatalogPriceAndInHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material]);

        var result = await sut.UseCase.Handle(ValidCommand(order, material, employee, 4), CancellationToken.None);

        sut.Items.Items.Count.ShouldBe(1);
        var item = sut.Items.Items[0];
        item.Quantity.ShouldBe(4);
        item.UnitPrice.ShouldBe(2.50m);
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        result.Id.ShouldBe(item.Id);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.EventType.ShouldBe(ServiceOrderEventType.ITEM_MATERIAL);
        history.ItemId.ShouldBe(item.Id);
        history.ItemDirection.ShouldBe(ServiceOrderItemDirection.IN);
        history.FromStatus.ShouldBeNull();

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_RepeatedActiveMaterial_ShouldSumQuantityInSameRowKeepingPrice()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var existing = ServiceOrderMaterial.Create(order.Id, material.Id, 3, 2.00m);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], items: [existing]);

        var result = await sut.UseCase.Handle(ValidCommand(order, material, employee, 2), CancellationToken.None);

        sut.Items.Items.Count.ShouldBe(1);
        sut.Items.Items[0].Id.ShouldBe(existing.Id);
        sut.Items.Items[0].Quantity.ShouldBe(5);
        sut.Items.Items[0].UnitPrice.ShouldBe(2.00m);
        result.Quantity.ShouldBe(5);

        sut.Histories.Histories.Count.ShouldBe(1);
        sut.Histories.Histories[0].ItemDirection.ShouldBe(ServiceOrderItemDirection.IN);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_RemovedMaterialBefore_ShouldReactivateSameRowWithInformedQuantityAndCurrentPrice()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var existing = ServiceOrderMaterial.Create(order.Id, material.Id, 3, 2.00m);
        existing.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], items: [existing]);

        await sut.UseCase.Handle(ValidCommand(order, material, employee, 4), CancellationToken.None);

        sut.Items.Items.Count.ShouldBe(1);
        sut.Items.Items[0].Id.ShouldBe(existing.Id);
        sut.Items.Items[0].Status.ShouldBe(EntityStatus.ACTIVE);
        sut.Items.Items[0].Quantity.ShouldBe(4);
        sut.Items.Items[0].UnitPrice.ShouldBe(2.50m);

        sut.Histories.Histories.Count.ShouldBe(1);
        sut.Histories.Histories[0].ItemDirection.ShouldBe(ServiceOrderItemDirection.IN);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_InactiveCatalogMaterial_ShouldThrowServiceOrderMaterialInactiveException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material]);

        var exception = await Should.ThrowAsync<ServiceOrderMaterialInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, material, employee, 2), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-material-inactive");
        sut.Items.Items.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownMaterial_ShouldThrowMaterialNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new AddMaterialToServiceOrderCommand(order.Id.ToString(), Guid.NewGuid().ToString(), 2, employee.Id.ToString());

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
    }

    [Fact]
    public async Task Handle_MalformedMaterialId_ShouldThrowInvalidMaterialIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new AddMaterialToServiceOrderCommand(order.Id.ToString(), "abc", 2, employee.Id.ToString());

        var exception = await Should.ThrowAsync<InvalidMaterialIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-id");
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
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material]);

        var exception = await Should.ThrowAsync<ServiceOrderNotEditableException>(
            () => sut.UseCase.Handle(ValidCommand(order, material, employee, 2), CancellationToken.None));

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
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material]);

        await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, material, employee, 2), CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(employees: [employee], materials: [material]);
        var command = new AddMaterialToServiceOrderCommand(Guid.NewGuid().ToString(), material.Id.ToString(), 2, employee.Id.ToString());

        await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AddingMaterial_ShouldNeverChangeStock()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var stockBefore = material.StockQuantity;
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material]);

        await sut.UseCase.Handle(ValidCommand(order, material, employee, 4), CancellationToken.None);

        material.StockQuantity.ShouldBe(stockBefore);
    }

    private static AddMaterialToServiceOrderCommand ValidCommand(ServiceOrder order, Material material, Employee employee, int quantity) =>
        new(order.Id.ToString(), material.Id.ToString(), quantity, employee.Id.ToString());

    private static AddMaterialToServiceOrderUseCaseSut CreateSut(
        ServiceOrder[]? orders = null,
        Employee[]? employees = null,
        Material[]? materials = null,
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

        var materialRepository = new FakeMaterialRepository();
        foreach (var material in materials ?? [])
        {
            materialRepository.WithMaterial(material);
        }

        var itemRepository = new FakeServiceOrderMaterialRepository();
        foreach (var item in items ?? [])
        {
            itemRepository.Add(item, CancellationToken.None);
        }

        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();
        var lookup = new ServiceOrderLookup(orderRepository, employeeRepository);

        var useCase = new AddMaterialToServiceOrderUseCase(
            lookup, materialRepository, itemRepository, itemRepository, historyRepository, unitWork);

        return new AddMaterialToServiceOrderUseCaseSut(useCase, itemRepository, historyRepository, unitWork);
    }

    private sealed record AddMaterialToServiceOrderUseCaseSut(
        AddMaterialToServiceOrderUseCase UseCase,
        FakeServiceOrderMaterialRepository Items,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
