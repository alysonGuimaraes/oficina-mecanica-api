using AutoGarage.Domain.Entities;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ShouldPersistMaterialWithZeroStock()
    {
        await using (var context = CreateContext())
        {
            await new MaterialRepository(context).Add(Material.Create("PARAFUSO", null, 2.50m, 3), CancellationToken.None);
            await context.SaveChangesAsync();
        }

        await using var query = CreateContext();
        var materials = await query.Materials.ToListAsync();

        materials.Count.ShouldBe(1);
        materials[0].Name.ShouldBe("PARAFUSO");
        materials[0].StockQuantity.ShouldBe(0);
        materials[0].StockMinimum.ShouldBe(3);
    }

    [Fact]
    public async Task ExistsByName_ExistingNormalizedName_ShouldReturnTrue()
    {
        await SaveAsync(Material.Create("PARAFUSO", null, 2.50m, 3));

        await using var context = CreateContext();
        var exists = await new MaterialRepository(context).ExistsByName("PARAFUSO", CancellationToken.None);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByName_DifferentName_ShouldReturnFalse()
    {
        await SaveAsync(Material.Create("PARAFUSO", null, 2.50m, 3));

        await using var context = CreateContext();
        var exists = await new MaterialRepository(context).ExistsByName("PORCA", CancellationToken.None);

        exists.ShouldBeFalse();
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
