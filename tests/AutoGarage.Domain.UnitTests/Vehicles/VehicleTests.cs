using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Vehicles;

public class VehicleTests
{
    [Fact]
    public void Create_ValidValues_ShouldBeActiveWithPlateTypeFromPlate()
    {
        var customerId = Guid.NewGuid();

        var vehicle = Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customerId);

        vehicle.Status.ShouldBe(EntityStatus.ACTIVE);
        vehicle.PlateType.ShouldBe(PlateType.MERCOSUL);
        vehicle.Plate.Value.ShouldBe("ABC1D23");
        vehicle.CustomerId.ShouldBe(customerId);
        vehicle.Id.ShouldNotBe(Guid.Empty);
        vehicle.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void DuplicateVehiclePlateException_ShouldHaveErrorTypeAndMessage()
    {
        var exception = new DuplicateVehiclePlateException("ABC1D23");

        exception.ErrorType.ShouldBe("duplicate-vehicle-plate");
        exception.Message.ShouldBe("The plate 'ABC1D23' is already registered for a vehicle.");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
    }

    [Fact]
    public void VehicleOwnerInactiveException_ShouldHaveErrorTypeAndMessage()
    {
        var exception = new VehicleOwnerInactiveException(Guid.NewGuid());

        exception.ErrorType.ShouldBe("vehicle-owner-inactive");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
    }
}
