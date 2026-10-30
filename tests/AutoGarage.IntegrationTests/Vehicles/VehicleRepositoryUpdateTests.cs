using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Vehicles;

public class VehicleRepositoryUpdateTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ExistsByPlateExcludingId_PlateOfAnotherVehicle_ShouldReturnTrue()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        var first = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id);
        var second = Vehicle.Create("GOL", "1.0", new Plate("ABC1234"), 2020, VehicleType.CAR, customer.Id);
        await SaveVehiclesAsync(first, second);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).ExistsByPlateExcludingId("ABC1234", first.Id, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByPlateExcludingId_OwnPlateInOtherFormat_ShouldReturnFalse()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id);
        await SaveVehiclesAsync(vehicle);

        await using var context = CreateContext();
        var result = await new VehicleRepository(context).ExistsByPlateExcludingId("abc-1d23", vehicle.Id, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Update_ChangedProfileAndOwner_ShouldPersistBrandPlateTypeAndCustomer()
    {
        var customer = CreateCustomer();
        await SaveCustomerAsync(customer);
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1234"), 2020, VehicleType.CAR, customer.Id);
        await SaveVehiclesAsync(vehicle);
        var newOwnerId = Guid.NewGuid();

        Vehicle loaded;

        await using (var readContext = CreateContext())
        {
            loaded = (await new VehicleRepository(readContext).GetById(vehicle.Id, CancellationToken.None))!;
        }

        loaded.UpdateProfile("FIAT", "UNO", new Plate("ABC1D23"), 2015);
        loaded.ChangeOwner(newOwnerId);

        await using (var writeContext = CreateContext())
        {
            await new VehicleRepository(writeContext).Update(loaded, CancellationToken.None);
            await writeContext.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var persisted = await verifyContext.Vehicles.AsNoTracking().SingleAsync(v => v.Id == vehicle.Id);

        persisted.Brand.ShouldBe("FIAT");
        persisted.Plate.Value.ShouldBe("ABC1D23");
        persisted.PlateType.ShouldBe(PlateType.MERCOSUL);
        persisted.Year.ShouldBe(2015);
        persisted.CustomerId.ShouldBe(newOwnerId);
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

    private async Task SaveVehiclesAsync(params Vehicle[] vehicles)
    {
        await using var context = CreateContext();
        context.Vehicles.AddRange(vehicles);
        await context.SaveChangesAsync();
    }
}
