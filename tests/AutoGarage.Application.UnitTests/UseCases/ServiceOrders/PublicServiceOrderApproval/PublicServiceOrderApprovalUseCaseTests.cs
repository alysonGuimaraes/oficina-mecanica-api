using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.PublicServiceOrderApproval;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.PublicServiceOrderApproval;

public class PublicServiceOrderApprovalUseCaseTests
{
    private const int DeadlineDays = 10;

    [Fact]
    public async Task Handle_Approved_WithEnoughStock_ShouldMoveToInExecutionAndDebit()
    {
        var order = AwaitingApproval();
        var material = StockedMaterial(5);
        var sut = CreateSut(order, [material], [Row(order, material, 2)], nowAfterBudgetDays: 1);

        var result = await sut.UseCase.Handle(Approve(order, true), CancellationToken.None);

        result.Message.ShouldBe("Budget approved.");
        sut.Orders.Orders.Single().Status.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
        material.StockQuantity.ShouldBe(3);
        sut.Transactions.Transactions.Count.ShouldBe(1);
        sut.Transactions.Transactions.Single().Type.ShouldBe(TransactionType.OUT);

        var history = sut.Histories.Histories.Single();
        history.EmployeeId.ShouldBeNull();
        history.IsCustomerAction.ShouldBeTrue();
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_Approved_WithoutEnoughStock_ShouldMoveToAwaitingMaterialsWithoutDebit()
    {
        var order = AwaitingApproval();
        var material = StockedMaterial(1);
        var sut = CreateSut(order, [material], [Row(order, material, 2)], nowAfterBudgetDays: 1);

        var result = await sut.UseCase.Handle(Approve(order, true), CancellationToken.None);

        result.Message.ShouldBe("Budget approved.");
        sut.Orders.Orders.Single().Status.ShouldBe(ServiceOrderStatus.AWAITING_MATERIALS);
        material.StockQuantity.ShouldBe(1);
        sut.Transactions.Transactions.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_Refused_ShouldMoveToFinishedWithoutStockChange()
    {
        var order = AwaitingApproval();
        var material = StockedMaterial(5);
        var sut = CreateSut(order, [material], [Row(order, material, 2)], nowAfterBudgetDays: 1);

        var result = await sut.UseCase.Handle(Approve(order, false), CancellationToken.None);

        result.Message.ShouldBe("Budget refused.");
        sut.Orders.Orders.Single().Status.ShouldBe(ServiceOrderStatus.FINISHED);
        material.StockQuantity.ShouldBe(5);
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ExpiredApproval_ShouldCancelPersistAndThrowExpired()
    {
        var order = AwaitingApproval();
        var sut = CreateSut(order, [], [], nowAfterBudgetDays: 11);

        await Should.ThrowAsync<ServiceOrderApprovalExpiredException>(
            () => sut.UseCase.Handle(Approve(order, true), CancellationToken.None));

        var stored = sut.Orders.Orders.Single();
        stored.Status.ShouldBe(ServiceOrderStatus.CANCELLED);
        stored.CancellationReason.ShouldBe("Approval deadline expired.");
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ExpiredRefusal_ShouldAlsoCancel()
    {
        var order = AwaitingApproval();
        var sut = CreateSut(order, [], [], nowAfterBudgetDays: 11);

        await Should.ThrowAsync<ServiceOrderApprovalExpiredException>(
            () => sut.UseCase.Handle(Approve(order, false), CancellationToken.None));

        sut.Orders.Orders.Single().Status.ShouldBe(ServiceOrderStatus.CANCELLED);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_NotAwaitingApproval_ShouldThrowConflictWithoutCommit()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut(order, [], [], nowAfterBudgetDays: 1);

        await Should.ThrowAsync<ServiceOrderNotAwaitingApprovalException>(
            () => sut.UseCase.Handle(Approve(order, true), CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_CalledTwice_SecondCallShouldBeConflict()
    {
        var order = AwaitingApproval();
        var material = StockedMaterial(5);
        var sut = CreateSut(order, [material], [Row(order, material, 2)], nowAfterBudgetDays: 1);

        await sut.UseCase.Handle(Approve(order, true), CancellationToken.None);

        await Should.ThrowAsync<ServiceOrderNotAwaitingApprovalException>(
            () => sut.UseCase.Handle(Approve(order, true), CancellationToken.None));

        material.StockQuantity.ShouldBe(3);
        sut.Transactions.Transactions.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowNotFound()
    {
        var sut = CreateSut(null, [], [], nowAfterBudgetDays: 1);

        await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(new PublicServiceOrderApprovalCommand(Guid.NewGuid().ToString(), true), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidOrderId_ShouldThrowBadRequest()
    {
        var sut = CreateSut(null, [], [], nowAfterBudgetDays: 1);

        await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.UseCase.Handle(new PublicServiceOrderApprovalCommand("abc", true), CancellationToken.None));
    }

    // Pedido em AWAITING_APPROVAL, com BudgetSentAt preenchido pelo próprio domínio.
    private static ServiceOrder AwaitingApproval()
    {
        var employeeId = Guid.NewGuid();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, employeeId);
        order.SendBudget(50m, employeeId);

        return order;
    }

    private static PublicServiceOrderApprovalCommand Approve(ServiceOrder order, bool approved) =>
        new(order.Id.ToString(), approved);

    private static Material StockedMaterial(int stock)
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 1);
        material.IncreaseStock(stock);
        return material;
    }

    private static ServiceOrderMaterial Row(ServiceOrder order, Material material, int quantity) =>
        ServiceOrderMaterial.Create(order.Id, material.Id, quantity, material.Price);

    private static ApprovalSut CreateSut(ServiceOrder? order, Material[] materials, ServiceOrderMaterial[] rows, int nowAfterBudgetDays)
    {
        var orderRepository = new FakeServiceOrderRepository();
        if (order is not null)
        {
            orderRepository.Add(order, CancellationToken.None);
        }

        var materialRepository = new FakeMaterialRepository();
        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var rowRepository = new FakeServiceOrderMaterialRepository();
        foreach (var row in rows)
        {
            rowRepository.Add(row, CancellationToken.None);
        }

        var transactionRepository = new FakeMaterialTransactionRepository();
        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();

        // "Agora" = BudgetSentAt + N dias, para testar o prazo sem esperar.
        var sentAt = order?.BudgetSentAt ?? DateTime.UtcNow;
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(sentAt, TimeSpan.Zero).AddDays(nowAfterBudgetDays));

        var lookup = new ServiceOrderLookup(orderRepository, new FakeEmployeeRepository());
        var stockDebit = new ServiceOrderStockDebit(rowRepository, materialRepository, materialRepository, transactionRepository);

        var useCase = new PublicServiceOrderApprovalUseCase(
            lookup,
            stockDebit,
            orderRepository,
            historyRepository,
            unitWork,
            new ServiceOrderApprovalSettings(DeadlineDays),
            timeProvider);

        return new ApprovalSut(useCase, orderRepository, historyRepository, transactionRepository, unitWork);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed record ApprovalSut(
        PublicServiceOrderApprovalUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeMaterialTransactionRepository Transactions,
        FakeUnitWork UnitWork);
}
