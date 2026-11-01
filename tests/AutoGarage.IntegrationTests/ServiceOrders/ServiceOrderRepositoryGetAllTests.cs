using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.ServiceOrders;

public class ServiceOrderRepositoryGetAllTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetAll_WithStatus_ShouldReturnOnlyOrdersWithThatStatus()
    {
        var received = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var inDiagnosis = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        inDiagnosis.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Guid.NewGuid());
        await SaveAsync(received);
        await SaveAsync(inDiagnosis);

        await using var query = CreateContext();
        var result = await new ServiceOrderRepository(query).GetAll(ServiceOrderStatus.IN_DIAGNOSIS, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(inDiagnosis.Id);
        result[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Fact]
    public async Task GetAll_NullStatus_ShouldReturnAllOrdersOrdered()
    {
        var first = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await SaveAsync(first);
        var second = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await SaveAsync(second);
        var third = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await SaveAsync(third);

        await using var query = CreateContext();
        var result = await new ServiceOrderRepository(query).GetAll(null, CancellationToken.None);

        result.Count.ShouldBe(3);
        var expectedIds = result
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Select(o => o.Id)
            .ToList();
        result.Select(o => o.Id).ToList().ShouldBe(expectedIds);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(ServiceOrder order)
    {
        await using var context = CreateContext();
        await new ServiceOrderRepository(context).Add(order, CancellationToken.None);
        await new WorkUnit(context).Commit();
    }
}
