using AutoGarage.Application.UseCases.Vehicles.GetVehicleById;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Vehicles;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.GetVehicleById;

public class GetVehicleByIdUseCaseTests
{
    [Fact]
    public async Task Handle_ExistingVehicle_ShouldReturnResponse()
    {
        var vehicle = CreateVehicle();
        var sut = CreateSut(vehicle);

        var result = await sut.UseCase.Handle(vehicle.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(vehicle.Id);
        result.Plate.ShouldBe("ABC1D23");
        result.PlateType.ShouldBe(PlateType.MERCOSUL);
        result.VehicleType.ShouldBe(VehicleType.CAR);
        result.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task Handle_InactiveVehicle_ShouldReturnWithInactiveStatus()
    {
        var vehicle = CreateVehicle();
        vehicle.Deactivate();
        var sut = CreateSut(vehicle);

        var result = await sut.UseCase.Handle(vehicle.Id.ToString(), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowVehicleNotFoundException()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();

        var exception = await Should.ThrowAsync<VehicleNotFoundException>(
            () => sut.UseCase.Handle(id.ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-not-found");
        exception.Message.ShouldBe($"Vehicle with id '{id}' was not found.");
        exception.ShouldBeAssignableTo<NotFoundDomainException>();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("12345")]
    public async Task Handle_MalformedId_ShouldThrowInvalidVehicleIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidVehicleIdException>(
            () => sut.UseCase.Handle(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-vehicle-id");
        exception.Message.ShouldBe("The provided id is invalid.");
        exception.ShouldBeAssignableTo<BadRequestDomainException>();
    }

    [Fact]
    public async Task Handle_UppercaseId_ShouldReturnSameVehicle()
    {
        var vehicle = CreateVehicle();
        var sut = CreateSut(vehicle);

        var result = await sut.UseCase.Handle(vehicle.Id.ToString().ToUpperInvariant(), CancellationToken.None);

        result.Id.ShouldBe(vehicle.Id);
    }

    private static Vehicle CreateVehicle()
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, Guid.NewGuid());
    }

    private static GetVehicleByIdUseCaseSut CreateSut(params Vehicle[] vehicles)
    {
        var repository = new FakeVehicleRepository();

        foreach (var vehicle in vehicles)
        {
            repository.WithVehicle(vehicle);
        }

        var useCase = new GetVehicleByIdUseCase(repository, MapperBuilder.Build());

        return new GetVehicleByIdUseCaseSut(useCase);
    }

    private sealed record GetVehicleByIdUseCaseSut(GetVehicleByIdUseCase UseCase);
}
