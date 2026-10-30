using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialTransactionRepositoryGetByMaterialIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetByMaterialId_ShouldReturnOnlyTransactionsOfThatMaterial()
    {
        var materialA = await SaveMaterialAsync("PARAFUSO");
        var materialB = await SaveMaterialAsync("PORCA");
        await AddTransactionAsync(materialA.Id, TransactionType.IN, 5, 5);
        await AddTransactionAsync(materialA.Id, TransactionType.OUT, 2, 3);
        await AddTransactionAsync(materialB.Id, TransactionType.IN, 9, 9);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetByMaterialId(materialA.Id, null, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.All(t => t.MaterialId == materialA.Id).ShouldBeTrue();
    }

    [Fact]
    public async Task GetByMaterialId_NullType_ShouldReturnBothTypes()
    {
        var material = await SaveMaterialAsync("PARAFUSO");
        await AddTransactionAsync(material.Id, TransactionType.IN, 5, 5);
        await AddTransactionAsync(material.Id, TransactionType.OUT, 2, 3);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetByMaterialId(material.Id, null, CancellationToken.None);

        result.Select(t => t.Type).ShouldBe([TransactionType.IN, TransactionType.OUT], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByMaterialId_TypeIn_ShouldReturnOnlyInTransactions()
    {
        var material = await SaveMaterialAsync("PARAFUSO");
        await AddTransactionAsync(material.Id, TransactionType.IN, 5, 5);
        await AddTransactionAsync(material.Id, TransactionType.OUT, 2, 3);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetByMaterialId(material.Id, TransactionType.IN, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(TransactionType.IN);
        result[0].Quantity.ShouldBe(5);
    }

    [Fact]
    public async Task GetByMaterialId_TypeOut_ShouldReturnOnlyOutTransactions()
    {
        var material = await SaveMaterialAsync("PARAFUSO");
        await AddTransactionAsync(material.Id, TransactionType.IN, 5, 5);
        await AddTransactionAsync(material.Id, TransactionType.OUT, 2, 3);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetByMaterialId(material.Id, TransactionType.OUT, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(TransactionType.OUT);
        result[0].Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task GetByMaterialId_ShouldOrderByCreatedAtThenId()
    {
        var material = await SaveMaterialAsync("PARAFUSO");
        var saved = new List<MaterialTransaction>
        {
            await AddTransactionAsync(material.Id, TransactionType.IN, 5, 5),
            await AddTransactionAsync(material.Id, TransactionType.OUT, 2, 3),
            await AddTransactionAsync(material.Id, TransactionType.IN, 1, 4)
        };

        var expected = saved
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Select(t => t.Id)
            .ToList();

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetByMaterialId(material.Id, null, CancellationToken.None);

        result.Select(t => t.Id).ShouldBe(expected);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task<Material> SaveMaterialAsync(string name)
    {
        var material = Material.Create(name, null, 2.50m, 3);

        await using var context = CreateContext();
        await new MaterialRepository(context).Add(material, CancellationToken.None);
        await new WorkUnit(context).Commit();

        return material;
    }

    private async Task<MaterialTransaction> AddTransactionAsync(Guid materialId, TransactionType type, int quantity, int balanceAfter)
    {
        var transaction = MaterialTransaction.Create(materialId, type, quantity, balanceAfter);

        await using var context = CreateContext();
        await new MaterialTransactionRepository(context).Add(transaction, CancellationToken.None);
        await new WorkUnit(context).Commit();

        return transaction;
    }
}
