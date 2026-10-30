using AutoGarage.Domain.Entities;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Services;

public class ServiceRepositoryUpdateTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ExistsByNameExcludingId_NameOfAnotherService_ShouldReturnTrue()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        await SaveAsync(service);
        await SaveAsync(Service.Create("TROCA DE ÓLEO", null, 150m));

        await using var context = CreateContext();
        var exists = await new ServiceRepository(context).ExistsByNameExcludingId("TROCA DE ÓLEO", service.Id, CancellationToken.None);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByNameExcludingId_OwnName_ShouldReturnFalse()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        await SaveAsync(service);

        await using var context = CreateContext();
        var exists = await new ServiceRepository(context).ExistsByNameExcludingId("ALINHAMENTO", service.Id, CancellationToken.None);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Update_ShouldPersistChangedProfile()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        await SaveAsync(service);

        await using (var context = CreateContext())
        {
            var repository = new ServiceRepository(context);
            var loaded = await repository.GetById(service.Id, CancellationToken.None);
            loaded!.UpdateProfile("TROCA DE ÓLEO", "Com filtro", 150.50m);

            await repository.Update(loaded, CancellationToken.None);
            await context.SaveChangesAsync();
        }

        await using var query = CreateContext();
        var result = await new ServiceRepository(query).GetById(service.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("TROCA DE ÓLEO");
        result.Description.ShouldBe("Com filtro");
        result.Price.ShouldBe(150.50m);
        result.UpdatedAt.ShouldNotBeNull();
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(Service service)
    {
        await using var context = CreateContext();
        context.Services.Add(service);
        await context.SaveChangesAsync();
    }
}
