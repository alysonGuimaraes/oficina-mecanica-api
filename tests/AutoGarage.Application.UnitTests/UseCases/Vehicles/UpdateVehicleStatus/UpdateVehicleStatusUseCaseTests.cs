using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.UseCases.Vehicles.UpdateVehicleStatus;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Vehicles;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.UpdateVehicleStatus;

public class UpdateVehicleStatusUseCaseTests
{
    [Fact]
    public async Task Handle_ActiveVehicleToInactive_ShouldDeactivate()
    {
        var vehicle = CreateVehicle();
        var sut = CreateSut(vehicle);

        var result = await sut.UseCase.Handle(new UpdateVehicleStatusCommand(vehicle.Id.ToString(), "INACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_InactiveVehicleToActive_ShouldActivate()
    {
        var vehicle = CreateVehicle();
        vehicle.Deactivate();
        var sut = CreateSut(vehicle);

        var result = await sut.UseCase.Handle(new UpdateVehicleStatusCommand(vehicle.Id.ToString(), "ACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_AlreadyActive_ShouldThrowVehicleAlreadyActiveException()
    {
        var vehicle = CreateVehicle();
        var sut = CreateSut(vehicle);

        var exception = await Should.ThrowAsync<VehicleAlreadyActiveException>(
            () => sut.UseCase.Handle(new UpdateVehicleStatusCommand(vehicle.Id.ToString(), "ACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-already-active");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_AlreadyInactive_ShouldThrowVehicleAlreadyInactiveException()
    {
        var vehicle = CreateVehicle();
        vehicle.Deactivate();
        var sut = CreateSut(vehicle);

        var exception = await Should.ThrowAsync<VehicleAlreadyInactiveException>(
            () => sut.UseCase.Handle(new UpdateVehicleStatusCommand(vehicle.Id.ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-already-inactive");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("  INACTIVE  ")]
    public async Task Handle_StatusCaseInsensitiveWithSpaces_ShouldDeactivate(string status)
    {
        var vehicle = CreateVehicle();
        var sut = CreateSut(vehicle);

        var result = await sut.UseCase.Handle(new UpdateVehicleStatusCommand(vehicle.Id.ToString(), status), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("ACTIVEE")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidVehicleStatusException(string status)
    {
        var vehicle = CreateVehicle();
        var sut = CreateSut(vehicle);

        var exception = await Should.ThrowAsync<InvalidVehicleStatusException>(
            () => sut.UseCase.Handle(new UpdateVehicleStatusCommand(vehicle.Id.ToString(), status), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-vehicle-status");
        exception.ShouldBeAssignableTo<BadRequestDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidVehicleIdException(string id)
    {
        var sut = CreateSut(CreateVehicle());

        var exception = await Should.ThrowAsync<InvalidVehicleIdException>(
            () => sut.UseCase.Handle(new UpdateVehicleStatusCommand(id, "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-vehicle-id");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowVehicleNotFoundException()
    {
        var sut = CreateSut(CreateVehicle());

        var exception = await Should.ThrowAsync<VehicleNotFoundException>(
            () => sut.UseCase.Handle(new UpdateVehicleStatusCommand(Guid.NewGuid().ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-not-found");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_MercosulVehicle_ShouldDeactivateAndKeepPlate()
    {
        var vehicle = CreateVehicle();
        var sut = CreateSut(vehicle);

        var result = await sut.UseCase.Handle(new UpdateVehicleStatusCommand(vehicle.Id.ToString(), "INACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        result.Plate.ShouldBe("ABC1D23");
        result.PlateType.ShouldBe(PlateType.MERCOSUL);
    }

    private static Vehicle CreateVehicle()
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, Guid.NewGuid());
    }

    private static UpdateVehicleStatusUseCaseSut CreateSut(params Vehicle[] vehicles)
    {
        var repository = new FakeVehicleRepository();

        foreach (var vehicle in vehicles)
        {
            repository.WithVehicle(vehicle);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateVehicleStatusUseCase(repository, repository, unitWork, MapperBuilder.Build());

        return new UpdateVehicleStatusUseCaseSut(useCase, unitWork);
    }

    private sealed record UpdateVehicleStatusUseCaseSut(
        UpdateVehicleStatusUseCase UseCase,
        FakeUnitWork UnitWork);
}
