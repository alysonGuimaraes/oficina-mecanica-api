using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Services;

public class ServiceRepositoryGetByIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetById_ExistingService_ShouldReturnIt()
    {
        var service = Service.Create("TROCA DE ÓLEO", null, 150m);
        await SaveAsync(service);

        await using var context = CreateContext();
        var result = await new ServiceRepository(context).GetById(service.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(service.Id);
        result.Name.ShouldBe("TROCA DE ÓLEO");
    }

    [Fact]
    public async Task GetById_InactiveService_ShouldReturnItWithInactiveStatus()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();
        await SaveAsync(service);

        await using var context = CreateContext();
        var result = await new ServiceRepository(context).GetById(service.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var result = await new ServiceRepository(context).GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
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
