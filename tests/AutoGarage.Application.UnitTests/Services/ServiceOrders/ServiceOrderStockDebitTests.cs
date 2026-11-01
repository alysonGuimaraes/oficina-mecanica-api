using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.ServiceOrders;

public class ServiceOrderStockDebitTests
{
    [Fact]
    public async Task DebitActiveMaterials_ShouldDecreaseStockAndRecordOutTransactionsLinkedToTheOrder()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var parafuso = StockedMaterial(10);
        var porca = StockedMaterial(5);
        var sut = CreateSut(materials: [parafuso, porca], rows: [Row(order, parafuso, 3), Row(order, porca, 2)]);

        await sut.Debit.DebitActiveMaterialsAsync(order, CancellationToken.None);

        parafuso.StockQuantity.ShouldBe(7);
        porca.StockQuantity.ShouldBe(3);

        sut.Transactions.Transactions.Count.ShouldBe(2);
        sut.Transactions.Transactions.ShouldAllBe(t => t.Type == TransactionType.OUT && t.ServiceOrderId == order.Id);
        sut.Transactions.Transactions.Single(t => t.MaterialId == parafuso.Id).BalanceAfter.ShouldBe(7);
        sut.Transactions.Transactions.Single(t => t.MaterialId == porca.Id).BalanceAfter.ShouldBe(3);
    }

    [Fact]
    public async Task DebitActiveMaterials_RemovedRow_ShouldNotBeDebited()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var material = StockedMaterial(10);
        var removed = Row(order, material, 4);
        removed.Deactivate();
        var sut = CreateSut(materials: [material], rows: [removed]);

        await sut.Debit.DebitActiveMaterialsAsync(order, CancellationToken.None);

        material.StockQuantity.ShouldBe(10);
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task DebitActiveMaterials_ExactStock_ShouldLeaveZero()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var material = StockedMaterial(4);
        var sut = CreateSut(materials: [material], rows: [Row(order, material, 4)]);

        await sut.Debit.DebitActiveMaterialsAsync(order, CancellationToken.None);

        material.StockQuantity.ShouldBe(0);
    }

    [Fact]
    public async Task DebitActiveMaterials_InsufficientForSecondMaterial_ShouldThrowAndDebitNothing()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var enough = StockedMaterial(10);
        var short_ = StockedMaterial(1);
        var sut = CreateSut(materials: [enough, short_], rows: [Row(order, enough, 3), Row(order, short_, 2)]);

        var exception = await Should.ThrowAsync<InsufficientStockException>(
            () => sut.Debit.DebitActiveMaterialsAsync(order, CancellationToken.None));

        exception.ErrorType.ShouldBe("insufficient-stock");
        exception.Message.ShouldContain(short_.Id.ToString());
        enough.StockQuantity.ShouldBe(10);
        short_.StockQuantity.ShouldBe(1);
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task DebitActiveMaterials_NoActiveRows_ShouldDoNothing()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut();

        await sut.Debit.DebitActiveMaterialsAsync(order, CancellationToken.None);

        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task DebitActiveMaterials_MaterialMissing_ShouldThrowMaterialNotFound()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut(rows: [ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 2, 1m)]);

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(
            () => sut.Debit.DebitActiveMaterialsAsync(order, CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task HasSufficientStockAsync_EnoughForAllActiveRows_ReturnsTrue()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var parafuso = StockedMaterial(10);
        var porca = StockedMaterial(5);
        var sut = CreateSut(materials: [parafuso, porca], rows: [Row(order, parafuso, 3), Row(order, porca, 2)]);

        var result = await sut.Debit.HasSufficientStockAsync(order, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task HasSufficientStockAsync_OneShortage_ReturnsFalse_AndNothingChanges()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var enough = StockedMaterial(10);
        var short_ = StockedMaterial(1);
        var sut = CreateSut(materials: [enough, short_], rows: [Row(order, enough, 3), Row(order, short_, 2)]);

        var result = await sut.Debit.HasSufficientStockAsync(order, CancellationToken.None);

        result.ShouldBeFalse();
        enough.StockQuantity.ShouldBe(10);
        short_.StockQuantity.ShouldBe(1);
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task HasSufficientStockAsync_IgnoresInactiveRows()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var material = StockedMaterial(10);
        var removed = Row(order, material, 50);
        removed.Deactivate();
        var sut = CreateSut(materials: [material], rows: [removed, Row(order, material, 4)]);

        var result = await sut.Debit.HasSufficientStockAsync(order, CancellationToken.None);

        result.ShouldBeTrue();
    }

    private static Material StockedMaterial(int stock)
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 1);
        material.IncreaseStock(stock);
        return material;
    }

    private static ServiceOrderMaterial Row(ServiceOrder order, Material material, int quantity) =>
        ServiceOrderMaterial.Create(order.Id, material.Id, quantity, material.Price);

    private static StockDebitSut CreateSut(Material[]? materials = null, ServiceOrderMaterial[]? rows = null)
    {
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

        var debit = new ServiceOrderStockDebit(rowRepository, materialRepository, materialRepository, transactionRepository);

        return new StockDebitSut(debit, transactionRepository);
    }

    private sealed record StockDebitSut(ServiceOrderStockDebit Debit, FakeMaterialTransactionRepository Transactions);
}
