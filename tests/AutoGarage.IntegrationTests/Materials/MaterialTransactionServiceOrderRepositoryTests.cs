using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialTransactionServiceOrderRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_WithServiceOrderId_ShouldPersistTheLink()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        await SaveMaterialAsync(material);
        var orderId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var transaction = MaterialTransaction.Create(material.Id, TransactionType.OUT, 2, 3, orderId);

            await new MaterialTransactionRepository(context).Add(transaction, CancellationToken.None);
            await new WorkUnit(context).Commit();
        }

        await using var query = CreateContext();
        var stored = await query.MaterialTransactions.SingleAsync();

        stored.ServiceOrderId.ShouldBe(orderId);
        stored.Type.ShouldBe(TransactionType.OUT);
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
