using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CancelServiceOrder;
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

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.CancelServiceOrder;

public class CancelServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_InExecution_ShouldCancelReturnStockAndRecordHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_EXECUTION, employee.Id);
        var material = StockedMaterial(7);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [Row(order, material, 3)]);

        await sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "  Cliente desistiu  ", employee.Id.ToString()), CancellationToken.None);

        var stored = sut.Orders.Orders[0];
        stored.Status.ShouldBe(ServiceOrderStatus.CANCELLED);
        stored.CancellationReason.ShouldBe("Cliente desistiu");
        material.StockQuantity.ShouldBe(10);

        sut.Transactions.Transactions.Count.ShouldBe(1);
        var transaction = sut.Transactions.Transactions[0];
        transaction.Type.ShouldBe(TransactionType.IN);
        transaction.ServiceOrderId.ShouldBe(order.Id);
        transaction.BalanceAfter.ShouldBe(10);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.FromStatus.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
        history.ToStatus.ShouldBe(ServiceOrderStatus.CANCELLED);
        history.EmployeeId.ShouldBe(employee.Id);

        sut.UnitWork.CommitCount.ShouldBe(1);

        sut.Notifier.Notified.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_InExecution_ShouldReturnResponseWithCancelledStatus()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_EXECUTION, employee.Id);
        var material = StockedMaterial(7);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [Row(order, material, 3)]);

        var result = await sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "Cliente desistiu", employee.Id.ToString()), CancellationToken.None);

        result.Status.ShouldBe(ServiceOrderStatus.CANCELLED);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    public async Task Handle_OriginWithoutDebit_ShouldCancelWithoutTouchingStock(ServiceOrderStatus from)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(from, employee.Id);
        var material = StockedMaterial(5);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [Row(order, material, 3)]);

        await sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "Cliente desistiu", employee.Id.ToString()), CancellationToken.None);

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.CANCELLED);
        material.StockQuantity.ShouldBe(5);
        sut.Transactions.Transactions.ShouldBeEmpty();
        sut.Histories.Histories[0].FromStatus.ShouldBe(from);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    public async Task Handle_NotCancellable_ShouldThrowInvalidTransitionAndChangeNothing(ServiceOrderStatus status)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(status, employee.Id);
        var reasonBefore = order.CancellationReason;
        var material = StockedMaterial(7);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [Row(order, material, 3)]);

        var exception = await Should.ThrowAsync<InvalidServiceOrderTransitionException>(
            () => sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "Cliente desistiu", employee.Id.ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-transition");
        sut.Orders.Orders[0].Status.ShouldBe(status);
        sut.Orders.Orders[0].CancellationReason.ShouldBe(reasonBefore);
        material.StockQuantity.ShouldBe(7);
        sut.Transactions.Transactions.ShouldBeEmpty();
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
        sut.Notifier.Notified.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_RemovedRow_ShouldNotBeReturned()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_EXECUTION, employee.Id);
        var material = StockedMaterial(7);
        var removed = Row(order, material, 3);
        removed.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], materials: [material], rows: [removed]);

        await sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "Cliente desistiu", employee.Id.ToString()), CancellationToken.None);

        material.StockQuantity.ShouldBe(7);
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.CANCELLED);
    }

    [Fact]
    public async Task Handle_InExecutionMaterialMissing_ShouldThrowMaterialNotFoundAndCommitNothing()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_EXECUTION, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee], rows: [ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 2, 1m)]);

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(
            () => sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "Cliente desistiu", employee.Id.ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
        sut.Transactions.Transactions.ShouldBeEmpty();
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
        sut.Notifier.Notified.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveAndKeepStatus()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.RECEIVED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "Cliente desistiu", employee.Id.ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-employee-inactive");
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.RECEIVED);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new CancelServiceOrderCommand(Guid.NewGuid().ToString(), "Cliente desistiu", employee.Id.ToString());

        var exception = await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-found");
    }

    [Fact]
    public async Task Handle_InvalidOrderId_ShouldThrowInvalidServiceOrderIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new CancelServiceOrderCommand("abc", "Cliente desistiu", employee.Id.ToString());

        var exception = await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-id");
    }

    [Fact]
    public async Task Handle_BlankReason_ShouldThrowErrorOnValidationWithoutCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.RECEIVED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new CancelServiceOrderCommand(order.Id.ToString(), "   ", employee.Id.ToString());

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);

        sut.Notifier.Notified.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_CommitFails_ShouldNotNotify()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_MATERIALS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        sut.UnitWork.ThrowOnCommit = new InvalidOperationException("database unavailable");

        await Should.ThrowAsync<InvalidOperationException>(
            () => sut.UseCase.Handle(new CancelServiceOrderCommand(order.Id.ToString(), "Cliente desistiu", employee.Id.ToString()), CancellationToken.None));

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

    private static CancelServiceOrderUseCaseSut CreateSut(
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
        var stockReturn = new ServiceOrderStockReturn(rowRepository, materialRepository, materialRepository, transactionRepository);

        var useCase = new CancelServiceOrderUseCase(
            lookup, stockReturn, orderRepository, historyRepository, unitWork, notifier, MapperBuilder.Build());

        return new CancelServiceOrderUseCaseSut(useCase, orderRepository, historyRepository, transactionRepository, unitWork, notifier);
    }

    private sealed record CancelServiceOrderUseCaseSut(
        CancelServiceOrderUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeMaterialTransactionRepository Transactions,
        FakeUnitWork UnitWork,
        FakeServiceOrderStatusNotifier Notifier);
}
