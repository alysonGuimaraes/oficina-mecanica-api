using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Vehicles;

public class VehicleRepositoryGetByStatusTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    private static readonly DateTime BaseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetByStatus_Active_ShouldReturnOnlyActiveVehicles()
    {
        var customer = await SaveCustomerAsync("john@autogarage.com");
        await SaveVehicleAsync(customer.Id, "ABC1D23", EntityStatus.ACTIVE);
        await SaveVehicleAsync(customer.Id, "ABC1234", EntityStatus.INACTIVE);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task GetByStatus_Inactive_ShouldReturnOnlyInactiveVehicles()
    {
        var customer = await SaveCustomerAsync("john@autogarage.com");
        await SaveVehicleAsync(customer.Id, "ABC1D23", EntityStatus.ACTIVE);
        await SaveVehicleAsync(customer.Id, "ABC1234", EntityStatus.INACTIVE);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetByStatus(EntityStatus.INACTIVE, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetByStatus_OrdersByCreatedAtThenId()
    {
        var customer = await SaveCustomerAsync("john@autogarage.com");
        var first = await SaveVehicleAsync(customer.Id, "ABC1D23", EntityStatus.ACTIVE, BaseDate.AddDays(2));
        var second = await SaveVehicleAsync(customer.Id, "ABC1234", EntityStatus.ACTIVE, BaseDate.AddDays(3));
        var third = await SaveVehicleAsync(customer.Id, "ABC1E23", EntityStatus.ACTIVE, BaseDate.AddDays(1));

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Select(v => v.Id).ShouldBe(new[] { third.Id, first.Id, second.Id });
    }

    [Fact]
    public async Task GetByStatus_SameCreatedAt_ShouldOrderByIdAscending()
    {
        var customer = await SaveCustomerAsync("john@autogarage.com");
        var a = await SaveVehicleAsync(customer.Id, "ABC1D23", EntityStatus.ACTIVE, BaseDate);
        var b = await SaveVehicleAsync(customer.Id, "ABC1234", EntityStatus.ACTIVE, BaseDate);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Select(v => v.Id).ShouldBe(new[] { a.Id, b.Id }.OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task GetByStatus_NoVehiclesWithStatus_ShouldReturnEmptyList()
    {
        var customer = await SaveCustomerAsync("john@autogarage.com");
        await SaveVehicleAsync(customer.Id, "ABC1D23", EntityStatus.ACTIVE);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetByStatus(EntityStatus.INACTIVE, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByStatus_VehiclesOfDifferentCustomers_ShouldReturnAllTogether()
    {
        var customerA = await SaveCustomerAsync("a@autogarage.com", "39053344705");
        var customerB = await SaveCustomerAsync("b@autogarage.com", "12345678909");
        await SaveVehicleAsync(customerA.Id, "ABC1D23", EntityStatus.ACTIVE);
        await SaveVehicleAsync(customerB.Id, "ABC1234", EntityStatus.ACTIVE);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetByStatus(EntityStatus.ACTIVE, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Select(v => v.CustomerId).Distinct().Count().ShouldBe(2);
    }

    private async Task<Customer> SaveCustomerAsync(string email, string document = "39053344705")
    {
        var customer = Customer.Create("John", new Document(document), email, "41", "999999999");

        await using var context = CreateContext();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return customer;
    }

    private async Task<Vehicle> SaveVehicleAsync(Guid customerId, string plate, EntityStatus status, DateTime? createdAt = null)
    {
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate(plate), 2020, VehicleType.CAR, customerId);

        await using var context = CreateContext();
        context.Vehicles.Add(vehicle);
        context.Entry(vehicle).Property(v => v.Status).CurrentValue = status;

        if (createdAt is not null)
        {
            context.Entry(vehicle).Property(v => v.CreatedAt).CurrentValue = createdAt.Value;
        }

        await context.SaveChangesAsync();

        return vehicle;
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);
}
