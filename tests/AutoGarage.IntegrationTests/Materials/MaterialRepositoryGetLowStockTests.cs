using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialRepositoryGetLowStockTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetLowStockByStatus_Active_ShouldReturnCriticalMaterialsInCreationOrder()
    {
        await SaveAsync(Material.Create("PARAFUSO", null, 2.50m, 3));
        await Task.Delay(20);
        await SaveAsync(Material.Create("ARRUELA", null, 0.50m, 0));
        await Task.Delay(20);
        await SaveAsync(Material.Create("PORCA", null, 1m, 2));

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetLowStockByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Select(m => m.Name).ShouldBe(new[] { "PARAFUSO", "PORCA" });
    }

    [Fact]
    public async Task GetLowStockByStatus_Inactive_ShouldReturnOnlyInactiveCriticalMaterials()
    {
        await SaveAsync(Material.Create("PARAFUSO", null, 2.50m, 3));
        await Task.Delay(20);
        await SaveAsync(CreateInactive("PORCA", 1m, 2));
        await Task.Delay(20);
        await SaveAsync(Material.Create("ARRUELA", null, 0.50m, 0));

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetLowStockByStatus(EntityStatus.INACTIVE, CancellationToken.None);

        result.Select(m => m.Name).ShouldBe(new[] { "PORCA" });
    }

    [Fact]
    public async Task GetLowStockByStatus_NoCriticalMaterials_ShouldReturnEmptyList()
    {
        await SaveAsync(Material.Create("ARRUELA", null, 0.50m, 0));

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetLowStockByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Material CreateInactive(string name, decimal price, int stockMinimum)
    {
        var material = Material.Create(name, null, price, stockMinimum);
        material.Deactivate();

        return material;
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(Material material)
    {
        await using var context = CreateContext();
        context.Materials.Add(material);
        await context.SaveChangesAsync();
    }
}
