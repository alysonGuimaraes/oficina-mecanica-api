using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.ServiceOrders;

public class ServiceOrderServiceRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ShouldPersistServiceLineActive()
    {
        var item = ServiceOrderService.Create(Guid.NewGuid(), Guid.NewGuid(), 80.00m);

        await SaveAsync(item);

        await using var query = CreateContext();
        var stored = await new ServiceOrderServiceRepository(query).GetById(item.Id, CancellationToken.None);

        stored.ShouldNotBeNull();
        stored.Price.ShouldBe(80.00m);
        stored.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task GetByServiceOrderAndServiceId_ShouldReturnLineEvenWhenInactive()
    {
        var serviceOrderId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var item = ServiceOrderService.Create(serviceOrderId, serviceId, 80.00m);
        item.Deactivate();
        await SaveAsync(item);

        await using var context = CreateContext();
        var result = await new ServiceOrderServiceRepository(context).GetByServiceOrderAndServiceId(serviceOrderId, serviceId, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Update_ShouldPersistDeactivationAndKeepRow()
    {
        var item = ServiceOrderService.Create(Guid.NewGuid(), Guid.NewGuid(), 80.00m);
        await SaveAsync(item);

        await using (var context = CreateContext())
        {
            var repository = new ServiceOrderServiceRepository(context);
            var loaded = await repository.GetById(item.Id, CancellationToken.None);
            loaded!.Deactivate();

            await repository.Update(loaded, CancellationToken.None);
            await new WorkUnit(context).Commit();
        }

        await using var query = CreateContext();
        var stored = await new ServiceOrderServiceRepository(query).GetById(item.Id, CancellationToken.None);

        stored.ShouldNotBeNull();
        stored.Status.ShouldBe(EntityStatus.INACTIVE);
        stored.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAllByServiceOrder_ShouldReturnActiveAndInactiveLines()
    {
        var serviceOrderId = Guid.NewGuid();
        var active = ServiceOrderService.Create(serviceOrderId, Guid.NewGuid(), 80.00m);
        var inactive = ServiceOrderService.Create(serviceOrderId, Guid.NewGuid(), 40.00m);
        inactive.Deactivate();
        await SaveAsync(active);
        await SaveAsync(inactive);

        await using var context = CreateContext();
        var result = await new ServiceOrderServiceRepository(context).GetAllByServiceOrder(serviceOrderId, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(ServiceOrderService item)
    {
        await using var context = CreateContext();
        await new ServiceOrderServiceRepository(context).Add(item, CancellationToken.None);
        await new WorkUnit(context).Commit();
    }
}
