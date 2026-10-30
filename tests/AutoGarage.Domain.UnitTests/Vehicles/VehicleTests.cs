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

    [Fact]
    public void UpdateProfile_OldPlateToAnotherOldPlate_ShouldChangeBrandModelPlateAndYearAndSetUpdatedAt()
    {
        var vehicle = CreateOldVehicle();

        vehicle.UpdateProfile("FIAT", "UNO", new Plate("XYZ9876"), 2015);

        vehicle.Brand.ShouldBe("FIAT");
        vehicle.Model.ShouldBe("UNO");
        vehicle.Plate.Value.ShouldBe("XYZ9876");
        vehicle.PlateType.ShouldBe(PlateType.OLD);
        vehicle.Year.ShouldBe(2015);
        vehicle.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateProfile_OldPlateToMercosul_ShouldRecalculatePlateType()
    {
        var vehicle = CreateOldVehicle();

        vehicle.UpdateProfile("GOL", "1.0", new Plate("ABC1D23"), 2020);

        vehicle.Plate.Value.ShouldBe("ABC1D23");
        vehicle.PlateType.ShouldBe(PlateType.MERCOSUL);
    }

    [Fact]
    public void UpdateProfile_MercosulPlateWithSamePlateInOtherFormat_ShouldAllowIt()
    {
        var vehicle = CreateVehicle();

        vehicle.UpdateProfile("GOL", "1.0", new Plate("abc-1d23"), 2020);

        vehicle.Plate.Value.ShouldBe("ABC1D23");
        vehicle.PlateType.ShouldBe(PlateType.MERCOSUL);
    }

    [Fact]
    public void UpdateProfile_MercosulPlateToAnotherPlate_ShouldThrowPlateUpdateNotAllowedException()
    {
        var vehicle = CreateVehicle();

        var exception = Should.Throw<PlateUpdateNotAllowedException>(
            () => vehicle.UpdateProfile("FIAT", "UNO", new Plate("XYZ9876"), 2015));

        exception.ErrorType.ShouldBe("plate-update-not-allowed");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        vehicle.Plate.Value.ShouldBe("ABC1D23");
        vehicle.Brand.ShouldBe("GOL");
    }

    [Fact]
    public void UpdateProfile_MercosulPlateToOldPlate_ShouldThrowPlateUpdateNotAllowedException()
    {
        var vehicle = CreateVehicle();

        var exception = Should.Throw<PlateUpdateNotAllowedException>(
            () => vehicle.UpdateProfile("GOL", "1.0", new Plate("ABC1234"), 2020));

        exception.ErrorType.ShouldBe("plate-update-not-allowed");
        vehicle.Plate.Value.ShouldBe("ABC1D23");
    }

    [Fact]
    public void UpdateProfile_ValidValues_ShouldKeepVehicleTypeCustomerStatusAndCreatedAt()
    {
        var vehicle = CreateOldVehicle();
        vehicle.Deactivate();
        var customerId = vehicle.CustomerId;
        var createdAt = vehicle.CreatedAt;

        vehicle.UpdateProfile("FIAT", "UNO", new Plate("XYZ9876"), 2015);

        vehicle.VehicleType.ShouldBe(VehicleType.CAR);
        vehicle.CustomerId.ShouldBe(customerId);
        vehicle.Status.ShouldBe(EntityStatus.INACTIVE);
        vehicle.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void ChangeOwner_NewCustomerId_ShouldChangeOwnerAndSetUpdatedAt()
    {
        var vehicle = CreateVehicle();
        var newCustomerId = Guid.NewGuid();

        vehicle.ChangeOwner(newCustomerId);

        vehicle.CustomerId.ShouldBe(newCustomerId);
        vehicle.UpdatedAt.ShouldNotBeNull();
        vehicle.Plate.Value.ShouldBe("ABC1D23");
    }

    private static Vehicle CreateVehicle()
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, Guid.NewGuid());
    }

    private static Vehicle CreateOldVehicle()
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1234"), 2020, VehicleType.CAR, Guid.NewGuid());
    }
}
