using AutoGarage.Application.UseCases.Vehicles.ListVehicles;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Vehicles;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;
using System.Reflection;

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.ListVehicles;

public class ListVehiclesUseCaseTests
{
    [Fact]
    public async Task Handle_NoStatus_ShouldReturnOnlyActiveVehicles()
    {
        var sut = CreateSut(CreateVehicle(EntityStatus.ACTIVE), CreateVehicle(EntityStatus.INACTIVE, "ABC1234"));

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public async Task Handle_BlankStatus_ShouldDefaultToActive(string status)
    {
        var sut = CreateSut(CreateVehicle(EntityStatus.ACTIVE), CreateVehicle(EntityStatus.INACTIVE, "ABC1234"));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("  INACTIVE  ")]
    public async Task Handle_StatusCaseInsensitiveWithSpaces_ShouldFilter(string status)
    {
        var sut = CreateSut(CreateVehicle(EntityStatus.ACTIVE), CreateVehicle(EntityStatus.INACTIVE, "ABC1234"));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("TRUCK")]
    [InlineData("0")]
    [InlineData("1")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidVehicleStatusException(string status)
    {
        var sut = CreateSut(CreateVehicle(EntityStatus.ACTIVE));

        var exception = await Should.ThrowAsync<InvalidVehicleStatusException>(
            () => sut.UseCase.Handle(status, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-vehicle-status");
        exception.ShouldBeAssignableTo<BadRequestDomainException>();
    }

    [Fact]
    public async Task Handle_NoVehiclesWithStatus_ShouldReturnEmptyList()
    {
        var sut = CreateSut(CreateVehicle(EntityStatus.ACTIVE));

        var result = await sut.UseCase.Handle("INACTIVE", CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Vehicle CreateVehicle(EntityStatus status, string plate = "ABC1D23")
    {
        var vehicle = Vehicle.Create("GOL", "1.0", new Plate(plate), 2020, VehicleType.CAR, Guid.NewGuid());

        // Vehicle ainda não tem método de inativação: o status de teste é definido direto, pois nenhum endpoint muda o status de veículo nesta fase.
        typeof(Vehicle)
            .GetProperty(nameof(Vehicle.Status), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(vehicle, status);

        return vehicle;
    }

    private static ListVehiclesUseCaseSut CreateSut(params Vehicle[] vehicles)
    {
        var repository = new FakeVehicleRepository();

        foreach (var vehicle in vehicles)
        {
            repository.WithVehicle(vehicle);
        }

        var useCase = new ListVehiclesUseCase(repository, MapperBuilder.Build());

        return new ListVehiclesUseCaseSut(useCase);
    }

    private sealed record ListVehiclesUseCaseSut(ListVehiclesUseCase UseCase);
}
