using AutoGarage.Domain.Entities;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Services;

public class ServiceRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ShouldPersistService()
    {
        await using (var context = CreateContext())
        {
            await new ServiceRepository(context).Add(Service.Create("TROCA DE ÓLEO", null, 150m), CancellationToken.None);
            await context.SaveChangesAsync();
        }

        await using var query = CreateContext();
        var services = await query.Services.ToListAsync();

        services.Count.ShouldBe(1);
        services[0].Name.ShouldBe("TROCA DE ÓLEO");
    }

    [Fact]
    public async Task ExistsByName_ExistingNormalizedName_ShouldReturnTrue()
    {
        await SaveServiceAsync(Service.Create("TROCA DE ÓLEO", null, 150m));

        await using var context = CreateContext();
        var exists = await new ServiceRepository(context).ExistsByName("TROCA DE ÓLEO", CancellationToken.None);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByName_DifferentName_ShouldReturnFalse()
    {
        await SaveServiceAsync(Service.Create("TROCA DE ÓLEO", null, 150m));

        await using var context = CreateContext();
        var exists = await new ServiceRepository(context).ExistsByName("ALINHAMENTO", CancellationToken.None);

        exists.ShouldBeFalse();
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveServiceAsync(Service service)
    {
        await using var context = CreateContext();
        context.Services.Add(service);
        await context.SaveChangesAsync();
    }
}
