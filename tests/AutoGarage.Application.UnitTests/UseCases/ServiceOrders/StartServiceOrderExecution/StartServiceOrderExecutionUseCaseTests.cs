using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.StartServiceOrderExecution;
using AutoGarage.CommonTestUtilities.Notifications;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.StartServiceOrderExecution;

public class StartServiceOrderExecutionUseCaseTests
{
    [Fact]
    public async Task Handle_AwaitingMaterialsWithStock_ShouldDebitAndMoveToInExecutionWithHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var material = StockedMaterial(10);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [Row(order, material, 3)]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        material.StockQuantity.ShouldBe(7);
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_EXECUTION);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.FromStatus.ShouldBe(ServiceOrderStatus.AWAITING_MATERIALS);
        history.ToStatus.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
        history.EmployeeId.ShouldBe(employee.Id);

        sut.Transactions.Transactions.Count.ShouldBe(1);
        sut.Transactions.Transactions[0].ServiceOrderId.ShouldBe(order.Id);

        sut.UnitWork.CommitCount.ShouldBe(1);

        sut.Notifier.Notified.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_AwaitingMaterialsWithStock_ShouldReturnResponseWithInExecutionStatus()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var material = StockedMaterial(10);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [Row(order, material, 3)]);

        var result = await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        result.Status.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldThrowInsufficientStockAndChangeNothing()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var enough = StockedMaterial(10);
        var short_ = StockedMaterial(1);
        var sut = CreateSut(
            orders: [order], employees: [employee], materials: [enough, short_],
            rows: [Row(order, enough, 3), Row(order, short_, 2)]);

        var exception = await Should.ThrowAsync<InsufficientStockException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("insufficient-stock");
        enough.StockQuantity.ShouldBe(10);
        short_.StockQuantity.ShouldBe(1);
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.AWAITING_MATERIALS);
        sut.Histories.Histories.ShouldBeEmpty();
        sut.Transactions.Transactions.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
        sut.Notifier.Notified.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    public async Task Handle_NotAwaitingMaterials_ShouldThrowInvalidTransitionAndChangeNothing(ServiceOrderStatus status)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(status, employee.Id);
        var material = StockedMaterial(10);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [Row(order, material, 3)]);

        var exception = await Should.ThrowAsync<InvalidServiceOrderTransitionException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-transition");
        material.StockQuantity.ShouldBe(10);
        sut.Orders.Orders[0].Status.ShouldBe(status);
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
        sut.Notifier.Notified.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_RemovedMaterialRow_ShouldNotBeDebited()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var material = StockedMaterial(10);
        var removed = Row(order, material, 4);
        removed.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [removed]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        material.StockQuantity.ShouldBe(10);
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
    }

    [Fact]
    public async Task Handle_OrderWithoutActiveMaterialRows_ShouldMoveWithoutDebit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveAndKeepStatus()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-employee-inactive");
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.AWAITING_MATERIALS);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new ServiceOrderEmployeeCommand(Guid.NewGuid().ToString(), employee.Id.ToString());

        var exception = await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-found");
    }

    [Fact]
    public async Task Handle_InvalidOrderId_ShouldThrowInvalidServiceOrderIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new ServiceOrderEmployeeCommand("abc", employee.Id.ToString());

        var exception = await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-id");
    }

    [Fact]
    public async Task Handle_BlankEmployeeId_ShouldThrowErrorOnValidationExceptionWithoutCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new ServiceOrderEmployeeCommand(order.Id.ToString(), "   ");

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);

        sut.Notifier.Notified.ShouldBeEmpty();
    }

    private static Material StockedMaterial(int stock)
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 1);
        material.IncreaseStock(stock);
        return material;
    }

    private static ServiceOrderMaterial Row(ServiceOrder order, Material material, int quantity) =>
        ServiceOrderMaterial.Create(order.Id, material.Id, quantity, material.Price);

    private static ServiceOrderEmployeeCommand ValidCommand(ServiceOrder order, Employee employee) =>
        new(order.Id.ToString(), employee.Id.ToString());

    private static StartExecutionSut CreateSut(
        ServiceOrder[]? orders = null,
        Employee[]? employees = null,
        Material[]? materials = null,
        ServiceOrderMaterial[]? rows = null)
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

        var rowRepository = new FakeServiceOrderMaterialRepository();
        foreach (var row in rows ?? [])
        {
            rowRepository.Add(row, CancellationToken.None);
        }

        var transactionRepository = new FakeMaterialTransactionRepository();
        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();
        var notifier = new FakeServiceOrderStatusNotifier();

        var lookup = new ServiceOrderLookup(orderRepository, employeeRepository);
        var debit = new ServiceOrderStockDebit(rowRepository, materialRepository, materialRepository, transactionRepository);

        var useCase = new StartServiceOrderExecutionUseCase(
            lookup, debit, orderRepository, historyRepository, unitWork, notifier, MapperBuilder.Build());

        return new StartExecutionSut(useCase, orderRepository, historyRepository, transactionRepository, unitWork, notifier);
    }

    private sealed record StartExecutionSut(
        StartServiceOrderExecutionUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeMaterialTransactionRepository Transactions,
        FakeUnitWork UnitWork,
        FakeServiceOrderStatusNotifier Notifier);
}
