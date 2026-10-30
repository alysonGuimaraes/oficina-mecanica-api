using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Vehicles.CreateVehicle;
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

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.CreateVehicle;

public class CreateVehicleUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateVehicleAndCommit()
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(
            new CreateVehicleCommand("gol", "1.0 mi total flex", "abc-1d23", 2020, VehicleType.CAR, customer.Id.ToString()),
            CancellationToken.None);

        result.Brand.ShouldBe("GOL");
        result.Model.ShouldBe("1.0 MI TOTAL FLEX");
        result.Plate.ShouldBe("ABC1D23");
        result.PlateType.ShouldBe(PlateType.MERCOSUL);
        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.Vehicles.Vehicles.Count.ShouldBe(1);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_BrandWithSpacesAndLowerCase_ShouldNormalize()
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(
            new CreateVehicleCommand("  mercedes   benz  ", "a200", "abc1234", 2020, VehicleType.CAR, customer.Id.ToString()),
            CancellationToken.None);

        result.Brand.ShouldBe("MERCEDES BENZ");
    }

    [Fact]
    public async Task Handle_InvalidPlate_ShouldThrowInvalidPlateException()
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<InvalidPlateException>(
            () => sut.UseCase.Handle(
                new CreateVehicleCommand("gol", "1.0", "ABC12345", 2020, VehicleType.CAR, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-plate");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_MalformedCustomerId_ShouldThrowInvalidCustomerIdException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidCustomerIdException>(
            () => sut.UseCase.Handle(
                new CreateVehicleCommand("gol", "1.0", "ABC1D23", 2020, VehicleType.CAR, "abc"),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-id");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownCustomer_ShouldThrowCustomerNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<CustomerNotFoundException>(
            () => sut.UseCase.Handle(
                new CreateVehicleCommand("gol", "1.0", "ABC1D23", 2020, VehicleType.CAR, Guid.NewGuid().ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-not-found");
    }

    [Fact]
    public async Task Handle_InactiveCustomer_ShouldThrowVehicleOwnerInactiveException()
    {
        var customer = CreateCustomer();
        customer.Deactivate();
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<VehicleOwnerInactiveException>(
            () => sut.UseCase.Handle(
                new CreateVehicleCommand("gol", "1.0", "ABC1D23", 2020, VehicleType.CAR, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-owner-inactive");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.Vehicles.Vehicles.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_PlateAlreadyRegisteredInAnotherFormat_ShouldThrowDuplicateVehiclePlateException()
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);
        sut.Vehicles.WithVehicle(Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customer.Id));

        var exception = await Should.ThrowAsync<DuplicateVehiclePlateException>(
            () => sut.UseCase.Handle(
                new CreateVehicleCommand("gol", "1.0", "abc 1d23", 2020, VehicleType.CAR, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-vehicle-plate");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidYear_ShouldThrowErrorOnValidationException()
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(
                new CreateVehicleCommand("gol", "1.0", "ABC1D23", 1899, VehicleType.CAR, customer.Id.ToString()),
                CancellationToken.None));

        exception.ErrorsMessages.ShouldContain($"The year field must be between 1900 and {DateTime.UtcNow.Year + 1}.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidDataWithUnknownCustomer_ShouldValidateBeforeLookup()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(
                new CreateVehicleCommand("", "1.0", "ABC1D23", 2020, VehicleType.CAR, Guid.NewGuid().ToString()),
                CancellationToken.None));
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create("John", new Document("39053344705"), "john@autogarage.com", "41", "999999999");
    }

    private static CreateVehicleUseCaseSut CreateSut(params Customer[] customers)
    {
        var customerRepository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            customerRepository.WithCustomer(customer);
        }

        var vehicleRepository = new FakeVehicleRepository();
        var unitWork = new FakeUnitWork();

        var useCase = new CreateVehicleUseCase(customerRepository, vehicleRepository, vehicleRepository, unitWork, new StringNormalizer(), MapperBuilder.Build());

        return new CreateVehicleUseCaseSut(useCase, vehicleRepository, unitWork);
    }

    private sealed record CreateVehicleUseCaseSut(
        CreateVehicleUseCase UseCase,
        FakeVehicleRepository Vehicles,
        FakeUnitWork UnitWork);
}
