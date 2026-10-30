using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Materials;

public class MaterialRepositoryGetByIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetById_ExistingMaterial_ShouldReturnIt()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        await SaveAsync(material);

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetById(material.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(material.Id);
        result.Name.ShouldBe("PARAFUSO");
    }

    [Fact]
    public async Task GetById_InactiveMaterial_ShouldReturnItWithInactiveStatus()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();
        await SaveAsync(material);

        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetById(material.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var result = await new MaterialRepository(context).GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
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
