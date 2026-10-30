using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Vehicles;

public class VehicleRepositoryUpdateStatusTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Update_Deactivated_ShouldPersistInactiveStatusAndKeepOtherFields()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id);
        await SaveVehicleAsync(vehicle);

        Vehicle loaded;

        await using (var readContext = CreateContext())
        {
            loaded = (await new VehicleRepository(readContext).GetById(vehicle.Id, CancellationToken.None))!;
        }

        loaded.Deactivate();

        await using (var writeContext = CreateContext())
        {
            await new VehicleRepository(writeContext).Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Vehicles.AsNoTracking().SingleAsync(v => v.Id == vehicle.Id);

        persisted.Status.ShouldBe(EntityStatus.INACTIVE);
        persisted.Plate.Value.ShouldBe("ABC1D23");
        persisted.VehicleType.ShouldBe(VehicleType.CAR);
        persisted.CustomerId.ShouldBe(customer.Id);
        persisted.Brand.ShouldBe("GOL");
    }

    [Fact]
    public async Task Update_Reactivated_ShouldPersistActiveStatus()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id);
        vehicle.Deactivate();
        await SaveVehicleAsync(vehicle);

        Vehicle loaded;

        await using (var readContext = CreateContext())
        {
            loaded = (await new VehicleRepository(readContext).GetById(vehicle.Id, CancellationToken.None))!;
        }

        loaded.Activate();

        await using (var writeContext = CreateContext())
        {
            await new VehicleRepository(writeContext).Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Vehicles.AsNoTracking().SingleAsync(v => v.Id == vehicle.Id);

        persisted.Status.ShouldBe(EntityStatus.ACTIVE);
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
