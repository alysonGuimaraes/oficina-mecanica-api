using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.ServiceOrders;

public class ServiceOrderRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ShouldPersistFieldsWithNullableBudgetEmpty()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var order = ServiceOrder.Create(customerId, vehicleId);

        await SaveAsync(order);

        await using var query = CreateContext();
        var stored = await new ServiceOrderRepository(query).GetById(order.Id, CancellationToken.None);

        stored.ShouldNotBeNull();
        stored.Status.ShouldBe(ServiceOrderStatus.RECEIVED);
        stored.CustomerId.ShouldBe(customerId);
        stored.VehicleId.ShouldBe(vehicleId);
        stored.BudgetTotal.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        await using var context = CreateContext();

        var result = await new ServiceOrderRepository(context).GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Update_ShouldPersistBudgetAndStatus()
    {
        var employeeId = Guid.NewGuid();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, employeeId);
        await SaveAsync(order);

        await using (var context = CreateContext())
        {
            var repository = new ServiceOrderRepository(context);
            var loaded = await repository.GetById(order.Id, CancellationToken.None);
            loaded!.SendBudget(150.00m, employeeId);

            await repository.Update(loaded, CancellationToken.None);
            await new WorkUnit(context).Commit();
        }

        await using var query = CreateContext();
        var stored = await new ServiceOrderRepository(query).GetById(order.Id, CancellationToken.None);

        stored!.Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
        stored.BudgetTotal.ShouldBe(150.00m);
        stored.BudgetSentAt.ShouldNotBeNull();
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
