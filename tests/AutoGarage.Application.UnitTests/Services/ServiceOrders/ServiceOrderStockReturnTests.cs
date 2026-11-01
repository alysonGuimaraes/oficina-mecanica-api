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

public class ServiceOrderStockReturnTests
{
    [Fact]
    public async Task ReturnActiveMaterials_ShouldIncreaseStockAndRecordInTransactionsLinkedToTheOrder()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var parafuso = StockedMaterial(7);
        var porca = StockedMaterial(3);
        var sut = CreateSut(materials: [parafuso, porca], rows: [Row(order, parafuso, 3), Row(order, porca, 2)]);

        await sut.Return.ReturnActiveMaterialsAsync(order, CancellationToken.None);

        parafuso.StockQuantity.ShouldBe(10);
        porca.StockQuantity.ShouldBe(5);

        sut.Transactions.Transactions.Count.ShouldBe(2);
        sut.Transactions.Transactions.ShouldAllBe(t => t.Type == TransactionType.IN && t.ServiceOrderId == order.Id);
        sut.Transactions.Transactions.Single(t => t.MaterialId == parafuso.Id).BalanceAfter.ShouldBe(10);
        sut.Transactions.Transactions.Single(t => t.MaterialId == porca.Id).BalanceAfter.ShouldBe(5);
    }

    [Fact]
    public async Task ReturnActiveMaterials_RemovedRow_ShouldNotBeReturned()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var material = StockedMaterial(10);
        var removed = Row(order, material, 4);
        removed.Deactivate();
        var sut = CreateSut(materials: [material], rows: [removed]);

        await sut.Return.ReturnActiveMaterialsAsync(order, CancellationToken.None);

        material.StockQuantity.ShouldBe(10);
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnActiveMaterials_NoActiveRows_ShouldDoNothing()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut();

        await sut.Return.ReturnActiveMaterialsAsync(order, CancellationToken.None);

        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnActiveMaterials_MaterialMissing_ShouldThrowMaterialNotFoundAndReturnNothing()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut(rows: [ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), 2, 1m)]);

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(
            () => sut.Return.ReturnActiveMaterialsAsync(order, CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
        sut.Transactions.Transactions.ShouldBeEmpty();
    }

    private static Material StockedMaterial(int stock)
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 1);
        material.IncreaseStock(stock);
        return material;
    }

    private static ServiceOrderMaterial Row(ServiceOrder order, Material material, int quantity) =>
        ServiceOrderMaterial.Create(order.Id, material.Id, quantity, material.Price);

    private static StockReturnSut CreateSut(Material[]? materials = null, ServiceOrderMaterial[]? rows = null)
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

        var stockReturn = new ServiceOrderStockReturn(rowRepository, materialRepository, materialRepository, transactionRepository);

        return new StockReturnSut(stockReturn, transactionRepository);
    }

    private sealed record StockReturnSut(ServiceOrderStockReturn Return, FakeMaterialTransactionRepository Transactions);
}
