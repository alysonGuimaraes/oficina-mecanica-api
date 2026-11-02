using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.ServiceOrders;

public class ServiceOrderRepositoryGetByCustomerTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetAllByCustomerExcludingStatus_ShouldReturnOnlyOrdersOfTheCustomerWithoutTheExcludedStatus()
    {
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var receivedA = ServiceOrder.Create(customerA, Guid.NewGuid());
        var deliveredA = ServiceOrder.Create(customerA, Guid.NewGuid());
        deliveredA.Cancel("motivo", employeeId);
        deliveredA.ChangeStatus(ServiceOrderStatus.DELIVERED, employeeId);
        var receivedB = ServiceOrder.Create(customerB, Guid.NewGuid());

        await SaveAsync(receivedA);
        await SaveAsync(deliveredA);
        await SaveAsync(receivedB);

        await using var query = CreateContext();
        var result = await new ServiceOrderRepository(query)
            .GetAllByCustomerExcludingStatus(customerA, ServiceOrderStatus.DELIVERED, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(receivedA.Id);
        result[0].CustomerId.ShouldBe(customerA);
        result[0].Status.ShouldBe(ServiceOrderStatus.RECEIVED);
    }

    [Fact]
    public async Task GetAllByCustomerExcludingStatus_CancelledOrders_ShouldBeIncluded()
    {
        var customer = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var cancelled = ServiceOrder.Create(customer, Guid.NewGuid());
        cancelled.Cancel("motivo", employeeId);
        await SaveAsync(cancelled);

        await using var query = CreateContext();
        var result = await new ServiceOrderRepository(query)
            .GetAllByCustomerExcludingStatus(customer, ServiceOrderStatus.DELIVERED, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(cancelled.Id);
        result[0].Status.ShouldBe(ServiceOrderStatus.CANCELLED);
    }

    [Fact]
    public async Task GetAllByCustomerExcludingStatus_ShouldOrderByCreatedAtThenId()
    {
        var customer = Guid.NewGuid();

        var first = ServiceOrder.Create(customer, Guid.NewGuid());
        await SaveAsync(first);
        var second = ServiceOrder.Create(customer, Guid.NewGuid());
        await SaveAsync(second);
        var third = ServiceOrder.Create(customer, Guid.NewGuid());
        await SaveAsync(third);

        await using var query = CreateContext();
        var result = await new ServiceOrderRepository(query)
            .GetAllByCustomerExcludingStatus(customer, ServiceOrderStatus.DELIVERED, CancellationToken.None);

        result.Count.ShouldBe(3);
        var expectedIds = result
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Select(o => o.Id)
            .ToList();
        result.Select(o => o.Id).ToList().ShouldBe(expectedIds);
    }

    [Fact]
    public async Task GetAllByCustomerExcludingStatus_ShouldOrderByStatusBeforeCreationDate()
    {
        var customer = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var olderCancelled = ServiceOrder.Create(customer, Guid.NewGuid());
        olderCancelled.Cancel("motivo", employeeId);
        await SaveAsync(olderCancelled);
        var newerReceived = ServiceOrder.Create(customer, Guid.NewGuid());
        await SaveAsync(newerReceived);

        await using var query = CreateContext();
        var result = await new ServiceOrderRepository(query)
            .GetAllByCustomerExcludingStatus(customer, ServiceOrderStatus.DELIVERED, CancellationToken.None);

        result.Select(o => o.Id).ToList().ShouldBe(new[] { newerReceived.Id, olderCancelled.Id }.ToList());
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
