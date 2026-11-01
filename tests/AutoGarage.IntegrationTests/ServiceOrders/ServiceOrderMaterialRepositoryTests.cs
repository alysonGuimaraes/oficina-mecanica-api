using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.ServiceOrders;

public class ServiceOrderMaterialRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ShouldPersistMaterialLine()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);

        await SaveAsync(item);

        await using var query = CreateContext();
        var stored = await new ServiceOrderMaterialRepository(query).GetById(item.Id, CancellationToken.None);

        stored.ShouldNotBeNull();
        stored.Quantity.ShouldBe(3);
        stored.UnitPrice.ShouldBe(2.50m);
        stored.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task GetByServiceOrderAndMaterialId_ShouldReturnLineEvenWhenInactive()
    {
        var serviceOrderId = Guid.NewGuid();
        var materialId = Guid.NewGuid();
        var item = ServiceOrderMaterial.Create(serviceOrderId, materialId, 3, 2.50m);
        item.Deactivate();
        await SaveAsync(item);

        await using var context = CreateContext();
        var result = await new ServiceOrderMaterialRepository(context).GetByServiceOrderAndMaterialId(serviceOrderId, materialId, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Update_ShouldPersistQuantitySumAndDeactivation()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);
        await SaveAsync(item);

        await using (var context = CreateContext())
        {
            var repository = new ServiceOrderMaterialRepository(context);
            var loaded = await repository.GetById(item.Id, CancellationToken.None);
            loaded!.AddQuantity(2);
            loaded.Deactivate();

            await repository.Update(loaded, CancellationToken.None);
            await new WorkUnit(context).Commit();
        }

        await using var query = CreateContext();
        var stored = await new ServiceOrderMaterialRepository(query).GetById(item.Id, CancellationToken.None);

        stored!.Quantity.ShouldBe(5);
        stored.Status.ShouldBe(EntityStatus.INACTIVE);
        stored.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAllByServiceOrder_ShouldReturnActiveAndInactiveLines()
    {
        var serviceOrderId = Guid.NewGuid();
        var active = ServiceOrderMaterial.Create(serviceOrderId, Guid.NewGuid(), 1, 2.50m);
        var inactive = ServiceOrderMaterial.Create(serviceOrderId, Guid.NewGuid(), 2, 3.00m);
        inactive.Deactivate();
        await SaveAsync(active);
        await SaveAsync(inactive);

        await using var context = CreateContext();
        var result = await new ServiceOrderMaterialRepository(context).GetAllByServiceOrder(serviceOrderId, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(ServiceOrderMaterial item)
    {
        await using var context = CreateContext();
        await new ServiceOrderMaterialRepository(context).Add(item, CancellationToken.None);
        await new WorkUnit(context).Commit();
    }
}
