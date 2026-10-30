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

    [Fact]
    public void Deactivate_ActiveVehicle_ShouldChangeStatusToInactiveAndSetUpdatedAt()
    {
        var vehicle = CreateVehicle();

        vehicle.Deactivate();

        vehicle.Status.ShouldBe(EntityStatus.INACTIVE);
        vehicle.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Activate_InactiveVehicle_ShouldChangeStatusToActive()
    {
        var vehicle = CreateVehicle();
        vehicle.Deactivate();

        vehicle.Activate();

        vehicle.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public void Activate_AlreadyActiveVehicle_ShouldThrowVehicleAlreadyActiveException()
    {
        var vehicle = CreateVehicle();

        var exception = Should.Throw<VehicleAlreadyActiveException>(() => vehicle.Activate());

        exception.ErrorType.ShouldBe("vehicle-already-active");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
    }

    [Fact]
    public void Deactivate_AlreadyInactiveVehicle_ShouldThrowVehicleAlreadyInactiveException()
    {
        var vehicle = CreateVehicle();
        vehicle.Deactivate();

        var exception = Should.Throw<VehicleAlreadyInactiveException>(() => vehicle.Deactivate());

        exception.ErrorType.ShouldBe("vehicle-already-inactive");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
    }

    private static Vehicle CreateVehicle()
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, Guid.NewGuid());
    }
}
