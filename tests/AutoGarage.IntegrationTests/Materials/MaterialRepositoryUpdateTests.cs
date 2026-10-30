using AutoGarage.Domain.Entities;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialRepositoryUpdateTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ExistsByNameExcludingId_NameOfAnotherMaterial_ShouldReturnTrue()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        await SaveAsync(material);
        await SaveAsync(Material.Create("PORCA", null, 1m, 1));

        await using var context = CreateContext();
        var exists = await new MaterialRepository(context).ExistsByNameExcludingId("PORCA", material.Id, CancellationToken.None);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByNameExcludingId_OwnName_ShouldReturnFalse()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        await SaveAsync(material);

        await using var context = CreateContext();
        var exists = await new MaterialRepository(context).ExistsByNameExcludingId("PARAFUSO", material.Id, CancellationToken.None);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Update_ShouldPersistChangedProfileAndKeepStock()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        await SaveAsync(material);

        await using (var context = CreateContext())
        {
            var repository = new MaterialRepository(context);
            var loaded = await repository.GetById(material.Id, CancellationToken.None);
            loaded!.UpdateProfile("PORCA", "Aço inox", 4.00m, 5);

            await repository.Update(loaded, CancellationToken.None);
            await context.SaveChangesAsync();
        }

        await using var query = CreateContext();
        var result = await new MaterialRepository(query).GetById(material.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("PORCA");
        result.Description.ShouldBe("Aço inox");
        result.Price.ShouldBe(4.00m);
        result.StockMinimum.ShouldBe(5);
        result.StockQuantity.ShouldBe(0);
        result.UpdatedAt.ShouldNotBeNull();
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
