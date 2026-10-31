using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.ServiceOrders;

public class ServiceOrderStatusHistoryRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ShouldPersistStatusChangeAndItemEvent()
    {
        var serviceOrderId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var statusChange = ServiceOrderStatusHistory.ForStatusChange(serviceOrderId, null, ServiceOrderStatus.RECEIVED, employeeId);
        var itemEvent = ServiceOrderStatusHistory.ForItemChange(serviceOrderId, ServiceOrderEventType.ITEM_SERVICE, itemId, ServiceOrderItemDirection.IN, employeeId);

        await SaveAsync(statusChange);
        await SaveAsync(itemEvent);

        await using var context = CreateContext();
        var result = await new ServiceOrderStatusHistoryRepository(context).GetAllByServiceOrder(serviceOrderId, CancellationToken.None);

        var stored = result.Single(h => h.EventType == ServiceOrderEventType.STATUS_CHANGE);
        stored.FromStatus.ShouldBeNull();
        stored.ToStatus.ShouldBe(ServiceOrderStatus.RECEIVED);
        stored.ItemId.ShouldBeNull();

        var storedItem = result.Single(h => h.EventType == ServiceOrderEventType.ITEM_SERVICE);
        storedItem.ItemId.ShouldBe(itemId);
        storedItem.ItemDirection.ShouldBe(ServiceOrderItemDirection.IN);
    }

    [Fact]
    public async Task GetAllByServiceOrder_ShouldOrderByCreatedAtThenId()
    {
        var serviceOrderId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var saved = new List<ServiceOrderStatusHistory>
        {
            ServiceOrderStatusHistory.ForStatusChange(serviceOrderId, null, ServiceOrderStatus.RECEIVED, employeeId),
            ServiceOrderStatusHistory.ForStatusChange(serviceOrderId, ServiceOrderStatus.RECEIVED, ServiceOrderStatus.IN_DIAGNOSIS, employeeId),
            ServiceOrderStatusHistory.ForStatusChange(serviceOrderId, ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.FINISHED, employeeId)
        };
        foreach (var history in saved)
        {
            await SaveAsync(history);
        }

        var expected = saved
            .OrderBy(h => h.CreatedAt)
            .ThenBy(h => h.Id)
            .Select(h => h.Id)
            .ToList();

        await using var context = CreateContext();
        var result = await new ServiceOrderStatusHistoryRepository(context).GetAllByServiceOrder(serviceOrderId, CancellationToken.None);

        result.Select(h => h.Id).ShouldBe(expected);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveAsync(ServiceOrderStatusHistory history)
    {
        await using var context = CreateContext();
        await new ServiceOrderStatusHistoryRepository(context).Add(history, CancellationToken.None);
        await new WorkUnit(context).Commit();
    }
}
