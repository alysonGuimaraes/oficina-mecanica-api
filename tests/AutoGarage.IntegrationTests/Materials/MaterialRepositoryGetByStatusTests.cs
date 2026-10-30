using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialRepositoryGetByStatusTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetByStatus_Active_ShouldReturnActiveMaterialsInCreationOrder()
    {
        await SaveAsync(Material.Create("PARAFUSO", null, 2.50m, 3));
        await Task.Delay(20);
        await SaveAsync(CreateInactive("PORCA", 1m, 1));
        await Task.Delay(20);
        await SaveAsync(Material.Create("ARRUELA", null, 0.50m, 2));

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Select(m => m.Name).ShouldBe(new[] { "PARAFUSO", "ARRUELA" });
    }

    [Fact]
    public async Task GetByStatus_Inactive_ShouldReturnOnlyInactiveMaterials()
    {
        await SaveAsync(Material.Create("PARAFUSO", null, 2.50m, 3));
        await Task.Delay(20);
        await SaveAsync(CreateInactive("PORCA", 1m, 1));
        await Task.Delay(20);
        await SaveAsync(Material.Create("ARRUELA", null, 0.50m, 2));

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetByStatus(EntityStatus.INACTIVE, CancellationToken.None);

        result.Select(m => m.Name).ShouldBe(new[] { "PORCA" });
    }

    [Fact]
    public async Task GetByStatus_NoMaterialsWithStatus_ShouldReturnEmptyList()
    {
        await SaveAsync(Material.Create("PARAFUSO", null, 2.50m, 3));

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetByStatus(EntityStatus.INACTIVE, CancellationToken.None);

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
