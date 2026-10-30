using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Services;

public class ServiceRepositoryGetByStatusTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetByStatus_Active_ShouldReturnActiveServicesInCreationOrder()
    {
        await SaveAsync(Service.Create("TROCA DE ÓLEO", null, 150m));
        await Task.Delay(20);
        await SaveAsync(CreateInactive("ALINHAMENTO", 80m));
        await Task.Delay(20);
        await SaveAsync(Service.Create("BALANCEAMENTO", null, 60m));

        await using var context = CreateContext();
        var result = await new ServiceRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Select(s => s.Name).ShouldBe(new[] { "TROCA DE ÓLEO", "BALANCEAMENTO" });
    }

    [Fact]
    public async Task GetByStatus_Inactive_ShouldReturnOnlyInactiveServices()
    {
        await SaveAsync(Service.Create("TROCA DE ÓLEO", null, 150m));
        await Task.Delay(20);
        await SaveAsync(CreateInactive("ALINHAMENTO", 80m));
        await Task.Delay(20);
        await SaveAsync(Service.Create("BALANCEAMENTO", null, 60m));

        await using var context = CreateContext();
        var result = await new ServiceRepository(context).GetByStatus(EntityStatus.INACTIVE, CancellationToken.None);

        result.Select(s => s.Name).ShouldBe(new[] { "ALINHAMENTO" });
    }

    [Fact]
    public async Task GetByStatus_NoServicesWithStatus_ShouldReturnEmptyList()
    {
        await SaveAsync(Service.Create("TROCA DE ÓLEO", null, 150m));

        await using var context = CreateContext();
        var result = await new ServiceRepository(context).GetByStatus(EntityStatus.INACTIVE, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Service CreateInactive(string name, decimal price)
    {
        var service = Service.Create(name, null, price);
        service.Deactivate();

        return service;
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
