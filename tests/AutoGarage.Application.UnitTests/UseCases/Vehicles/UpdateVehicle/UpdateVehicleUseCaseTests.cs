using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Vehicles.UpdateVehicle;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.CommonTestUtilities.Repositories.Vehicles;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.UpdateVehicle;

public class UpdateVehicleUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateVehicleAndCommit()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateOldVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var result = await sut.UseCase.Handle(
            new UpdateVehicleCommand(vehicle.Id.ToString(), "fiat", "uno", "abc-1d23", 2015, customer.Id.ToString()),
            CancellationToken.None);

        result.Brand.ShouldBe("FIAT");
        result.Model.ShouldBe("UNO");
        result.Plate.ShouldBe("ABC1D23");
        result.Year.ShouldBe(2015);
        result.PlateType.ShouldBe(PlateType.MERCOSUL);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_SameMercosulPlateInOtherFormat_ShouldNotConflictWithItself()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateMercosulVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var result = await sut.UseCase.Handle(
            new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "abc 1d23", 2020, customer.Id.ToString()),
            CancellationToken.None);

        result.Plate.ShouldBe("ABC1D23");
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_MercosulPlateChangedToAnotherPlate_ShouldThrowPlateUpdateNotAllowedException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateMercosulVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var exception = await Should.ThrowAsync<PlateUpdateNotAllowedException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "XYZ9876", 2020, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("plate-update-not-allowed");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
        vehicle.Plate.Value.ShouldBe("ABC1D23");
    }

    [Fact]
    public async Task Handle_MercosulPlateChangedToOldPlate_ShouldThrowPlateUpdateNotAllowedException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateMercosulVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var exception = await Should.ThrowAsync<PlateUpdateNotAllowedException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "ABC1234", 2020, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("plate-update-not-allowed");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_OldPlateChangedToAnotherOldPlate_ShouldUpdate()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateOldVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var result = await sut.UseCase.Handle(
            new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "XYZ9876", 2020, customer.Id.ToString()),
            CancellationToken.None);

        result.Plate.ShouldBe("XYZ9876");
        result.PlateType.ShouldBe(PlateType.OLD);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_PlateOfAnotherVehicle_ShouldThrowDuplicateVehiclePlateException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicleA = CreateOldVehicle(customer.Id);
        var vehicleB = CreateMercosulVehicle(customer.Id);
        var sut = CreateSut([customer], vehicleA, vehicleB);

        var exception = await Should.ThrowAsync<DuplicateVehiclePlateException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicleA.Id.ToString(), "gol", "1.0", "abc1d23", 2020, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-vehicle-plate");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidPlate_ShouldThrowInvalidPlateException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateOldVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var exception = await Should.ThrowAsync<InvalidPlateException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "ABC12345", 2020, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-plate");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ChangeToActiveCustomer_ShouldChangeOwner()
    {
        var customerA = CreateActiveCustomer("39053344705");
        var customerB = CreateActiveCustomer("12345678909");
        var vehicle = CreateOldVehicle(customerA.Id);
        var sut = CreateSut([customerA, customerB], vehicle);

        var result = await sut.UseCase.Handle(
            new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "ABC1234", 2020, customerB.Id.ToString()),
            CancellationToken.None);

        result.CustomerId.ShouldBe(customerB.Id);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_UnknownCustomer_ShouldThrowCustomerNotFoundException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateOldVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var exception = await Should.ThrowAsync<CustomerNotFoundException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "ABC1234", 2020, Guid.NewGuid().ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-not-found");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveCustomer_ShouldThrowVehicleOwnerInactiveException()
    {
        var customer = CreateActiveCustomer("39053344705");
        customer.Deactivate();
        var vehicle = CreateOldVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var exception = await Should.ThrowAsync<VehicleOwnerInactiveException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "ABC1234", 2020, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-owner-inactive");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_MalformedCustomerId_ShouldThrowInvalidCustomerIdException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateOldVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var exception = await Should.ThrowAsync<InvalidCustomerIdException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "ABC1234", 2020, "abc"),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-id");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_MalformedId_ShouldThrowInvalidVehicleIdException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var sut = CreateSut([customer]);

        var exception = await Should.ThrowAsync<InvalidVehicleIdException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand("abc", "gol", "1.0", "ABC1234", 2020, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-vehicle-id");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowVehicleNotFoundException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var sut = CreateSut([customer]);

        var exception = await Should.ThrowAsync<VehicleNotFoundException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(Guid.NewGuid().ToString(), "gol", "1.0", "ABC1234", 2020, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-not-found");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveVehicle_ShouldUpdateAndKeepStatus()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateOldVehicle(customer.Id);
        vehicle.Deactivate();
        var sut = CreateSut([customer], vehicle);

        var result = await sut.UseCase.Handle(
            new UpdateVehicleCommand(vehicle.Id.ToString(), "fiat", "uno", "XYZ9876", 2015, customer.Id.ToString()),
            CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        result.Brand.ShouldBe("FIAT");
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_InvalidYear_ShouldThrowErrorOnValidationException()
    {
        var customer = CreateActiveCustomer("39053344705");
        var vehicle = CreateOldVehicle(customer.Id);
        var sut = CreateSut([customer], vehicle);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(vehicle.Id.ToString(), "gol", "1.0", "ABC1234", 1899, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorsMessages.ShouldContain($"The year field must be between 1900 and {DateTime.UtcNow.Year + 1}.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidDataWithUnknownId_ShouldValidateBeforeLookup()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(
                new UpdateVehicleCommand(Guid.NewGuid().ToString(), "", "1.0", "ABC1234", 2020, Guid.NewGuid().ToString()),
                CancellationToken.None));
    }

    private static Customer CreateActiveCustomer(string document)
    {
        return Customer.Create("John", new Document(document), $"{document}@autogarage.com", "41", "999999999");
    }

    private static Vehicle CreateOldVehicle(Guid customerId)
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1234"), 2020, VehicleType.CAR, customerId);
    }

    private static Vehicle CreateMercosulVehicle(Guid customerId)
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customerId);
    }

    private static UpdateVehicleUseCaseSut CreateSut(Customer[]? customers = null, params Vehicle[] vehicles)
    {
        var customerRepository = new FakeCustomerRepository();

        foreach (var customer in customers ?? [])
        {
            customerRepository.WithCustomer(customer);
        }

        var vehicleRepository = new FakeVehicleRepository();

        foreach (var vehicle in vehicles)
        {
            vehicleRepository.WithVehicle(vehicle);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateVehicleUseCase(vehicleRepository, vehicleRepository, customerRepository, unitWork, new StringNormalizer(), MapperBuilder.Build());

        return new UpdateVehicleUseCaseSut(useCase, unitWork);
    }

    private sealed record UpdateVehicleUseCaseSut(
        UpdateVehicleUseCase UseCase,
        FakeUnitWork UnitWork);
}
