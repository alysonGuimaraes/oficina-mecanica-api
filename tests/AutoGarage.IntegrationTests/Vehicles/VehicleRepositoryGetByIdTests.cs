using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Vehicles;

public class VehicleRepositoryGetByIdTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task GetById_ExistingVehicle_ShouldReturnIt()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id);
        await SaveVehicleAsync(vehicle);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetById(vehicle.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(vehicle.Id);
        result.Plate.Value.ShouldBe("ABC1D23");
    }

    [Fact]
    public async Task GetById_InactiveVehicle_ShouldReturnItWithInactiveStatus()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id);
        vehicle.Deactivate();
        await SaveVehicleAsync(vehicle);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetById(vehicle.Id, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        await using var context = CreateContext();
        var result = await new VehicleRepository(context).GetById(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeNull();
    }

    private static Customer CreateCustomer() =>
        Customer.Create("John", new Document("39053344705"), "john@autogarage.com", "41", "999999999");

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private async Task SaveCustomerAsync(Customer customer)
    {
        await using var context = CreateContext();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
    }

    private async Task SaveVehicleAsync(Vehicle vehicle)
    {
        await using var context = CreateContext();
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();
    }
}
