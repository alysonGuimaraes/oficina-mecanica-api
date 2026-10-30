using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Materials;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialTransactionRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ShouldPersistTransactionFields()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        await SaveMaterialAsync(material);

        await using (var context = CreateContext())
        {
            var transaction = MaterialTransaction.Create(material.Id, TransactionType.IN, 5, 5);

            await new MaterialTransactionRepository(context).Add(transaction, CancellationToken.None);
            await new WorkUnit(context).Commit();
        }

        await using var query = CreateContext();
        var transactions = await query.MaterialTransactions.ToListAsync();

        transactions.Count.ShouldBe(1);
        transactions[0].MaterialId.ShouldBe(material.Id);
        transactions[0].Type.ShouldBe(TransactionType.IN);
        transactions[0].Quantity.ShouldBe(5);
        transactions[0].BalanceAfter.ShouldBe(5);
    }

    [Fact]
    public async Task StockEntry_ShouldPersistStockAndTransactionTogetherInOneSave()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        await SaveMaterialAsync(material);

        await using (var context = CreateContext())
        {
            var materialRepository = new MaterialRepository(context);
            var loaded = await materialRepository.GetById(material.Id, CancellationToken.None);
            loaded!.IncreaseStock(5);

            await materialRepository.Update(loaded, CancellationToken.None);
            await new MaterialTransactionRepository(context)
                .Add(MaterialTransaction.Create(material.Id, TransactionType.IN, 5, 5), CancellationToken.None);
            await new WorkUnit(context).Commit();
        }

        await using var query = CreateContext();
        var stored = await new MaterialRepository(query).GetById(material.Id, CancellationToken.None);
        var transactions = await query.MaterialTransactions.ToListAsync();

        stored!.StockQuantity.ShouldBe(5);
        transactions.Count.ShouldBe(1);
        transactions[0].BalanceAfter.ShouldBe(5);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveMaterialAsync(Material material)
    {
        await using var context = CreateContext();
        await new MaterialRepository(context).Add(material, CancellationToken.None);
        await new WorkUnit(context).Commit();
    }
}
