using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Vehicles;

public class VehicleRepositoryTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Add_ValidVehicle_ShouldPersistAllFields()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);

        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id);

        await using (var writeContext = CreateContext())
        {
            await new VehicleRepository(writeContext).Add(vehicle, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Vehicles.AsNoTracking().SingleAsync(v => v.Id == vehicle.Id);

        persisted.Brand.ShouldBe("GOL");
        persisted.Plate.Value.ShouldBe("ABC1D23");
        persisted.PlateType.ShouldBe(PlateType.MERCOSUL);
        persisted.VehicleType.ShouldBe(VehicleType.CAR);
        persisted.Year.ShouldBe(2020);
        persisted.CustomerId.ShouldBe(customer.Id);
        persisted.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task ExistsByPlate_SavedPlate_ShouldReturnTrue()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        await SaveVehicleAsync(Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id));

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).ExistsByPlate("ABC1D23", CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByPlate_UnknownPlate_ShouldReturnFalse()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        await SaveVehicleAsync(Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id));

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).ExistsByPlate("ABC1234", CancellationToken.None);

        result.ShouldBeFalse();
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
