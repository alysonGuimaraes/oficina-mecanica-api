using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Materials;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialTransactionRepositoryGetAllAndByIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetAll_NullType_ShouldReturnTransactionsOfAllMaterials()
    {
        var materialA = await SaveMaterialAsync("PARAFUSO");
        var materialB = await SaveMaterialAsync("PORCA");
        await AddTransactionAsync(materialA.Id, TransactionType.IN, 5, 5);
        await AddTransactionAsync(materialB.Id, TransactionType.IN, 9, 9);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetAll(null, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Select(t => t.Quantity).ShouldBe([5, 9], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAll_TypeOut_ShouldReturnOnlyOutTransactionsOfAllMaterials()
    {
        var materialA = await SaveMaterialAsync("PARAFUSO");
        var materialB = await SaveMaterialAsync("PORCA");
        await AddTransactionAsync(materialA.Id, TransactionType.IN, 5, 5);
        await AddTransactionAsync(materialA.Id, TransactionType.OUT, 2, 3);
        await AddTransactionAsync(materialB.Id, TransactionType.OUT, 3, 6);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetAll(TransactionType.OUT, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.All(t => t.Type == TransactionType.OUT).ShouldBeTrue();
    }

    [Fact]
    public async Task GetAll_ShouldOrderByCreatedAtThenId()
    {
        var materialA = await SaveMaterialAsync("PARAFUSO");
        var materialB = await SaveMaterialAsync("PORCA");
        var saved = new List<MaterialTransaction>
        {
            await AddTransactionAsync(materialA.Id, TransactionType.IN, 5, 5),
            await AddTransactionAsync(materialB.Id, TransactionType.OUT, 2, 3),
            await AddTransactionAsync(materialA.Id, TransactionType.IN, 1, 4)
        };

        var expected = saved
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Select(t => t.Id)
            .ToList();

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetAll(null, CancellationToken.None);

        result.Select(t => t.Id).ShouldBe(expected);
    }

    [Fact]
    public async Task GetById_ExistingId_ShouldReturnTransaction()
    {
        var material = await SaveMaterialAsync("PARAFUSO");
        var saved = await AddTransactionAsync(material.Id, TransactionType.IN, 5, 5);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetById(saved.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(saved.Id);
        result.MaterialId.ShouldBe(material.Id);
        result.Type.ShouldBe(TransactionType.IN);
        result.Quantity.ShouldBe(5);
        result.BalanceAfter.ShouldBe(5);
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        var material = await SaveMaterialAsync("PARAFUSO");
        await AddTransactionAsync(material.Id, TransactionType.IN, 5, 5);

        await using var context = CreateContext();
        var result = await new MaterialTransactionRepository(context).GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
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
