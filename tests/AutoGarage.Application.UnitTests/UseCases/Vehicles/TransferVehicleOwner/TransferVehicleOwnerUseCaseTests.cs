using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.UseCases.Vehicles.TransferVehicleOwner;
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

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.TransferVehicleOwner;

public class TransferVehicleOwnerUseCaseTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public async Task Handle_CpfFormattedOrNot_ShouldTransferToSameCustomer(string document)
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var customerB = CreateCustomer("12ABC34501DE35", "Company", "company@autogarage.com");
        var vehicle = CreateVehicle(customerB.Id);
        var sut = CreateSut([customerA, customerB], vehicle);

        var result = await sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), document), CancellationToken.None);

        result.CustomerId.ShouldBe(customerA.Id);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Theory]
    [InlineData("12ABC34501DE35")]
    [InlineData("12.abc.345/01de-35")]
    public async Task Handle_CnpjInAnyFormattingOrCase_ShouldTransferToCustomer(string document)
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var customerB = CreateCustomer("12ABC34501DE35", "Company", "company@autogarage.com");
        var vehicle = CreateVehicle(customerA.Id);
        var sut = CreateSut([customerA, customerB], vehicle);

        var result = await sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), document), CancellationToken.None);

        result.CustomerId.ShouldBe(customerB.Id);
    }

    [Fact]
    public async Task Handle_TransferToCurrentOwner_ShouldSucceed()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var vehicle = CreateVehicle(customerA.Id);
        var sut = CreateSut([customerA], vehicle);

        var result = await sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), "52998224725"), CancellationToken.None);

        result.CustomerId.ShouldBe(customerA.Id);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_TransferKeepsOtherFields()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var customerB = CreateCustomer("12ABC34501DE35", "Company", "company@autogarage.com");
        var vehicle = CreateVehicle(customerA.Id);
        var sut = CreateSut([customerA, customerB], vehicle);

        var result = await sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), "12ABC34501DE35"), CancellationToken.None);

        result.Brand.ShouldBe("GOL");
        result.Plate.ShouldBe("ABC1D23");
        result.Status.ShouldBe(EntityStatus.ACTIVE);
        result.Year.ShouldBe(2020);
    }

    [Fact]
    public async Task Handle_InvalidDocument_ShouldThrowInvalidDocumentException()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var vehicle = CreateVehicle(customerA.Id);
        var sut = CreateSut([customerA], vehicle);

        var exception = await Should.ThrowAsync<InvalidDocumentException>(
            () => sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), "529.982.247-26"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-document");
        sut.UnitWork.CommitCount.ShouldBe(0);
        vehicle.CustomerId.ShouldBe(customerA.Id);
    }

    [Fact]
    public async Task Handle_DocumentWithoutCustomer_ShouldThrowCustomerDocumentNotFoundException()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var vehicle = CreateVehicle(customerA.Id);
        var sut = CreateSut([customerA], vehicle);

        var exception = await Should.ThrowAsync<CustomerDocumentNotFoundException>(
            () => sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), "11222333000181"), CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-document-not-found");
        exception.ShouldBeAssignableTo<NotFoundDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveNewOwner_ShouldThrowVehicleOwnerInactiveException()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var customerC = CreateCustomer("39053344705", "Inactive", "inactive@autogarage.com");
        customerC.Deactivate();
        var vehicle = CreateVehicle(customerA.Id);
        var sut = CreateSut([customerA, customerC], vehicle);

        var exception = await Should.ThrowAsync<VehicleOwnerInactiveException>(
            () => sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), "39053344705"), CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-owner-inactive");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
        vehicle.CustomerId.ShouldBe(customerA.Id);
    }

    [Fact]
    public async Task Handle_UnknownVehicle_ShouldThrowVehicleNotFoundException()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var sut = CreateSut([customerA]);

        var exception = await Should.ThrowAsync<VehicleNotFoundException>(
            () => sut.UseCase.Handle(new TransferVehicleOwnerCommand(Guid.NewGuid().ToString(), "52998224725"), CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-not-found");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_MalformedId_ShouldThrowInvalidVehicleIdException()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var sut = CreateSut([customerA]);

        var exception = await Should.ThrowAsync<InvalidVehicleIdException>(
            () => sut.UseCase.Handle(new TransferVehicleOwnerCommand("abc", "52998224725"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-vehicle-id");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_EmptyDocument_ShouldThrowErrorOnValidationException()
    {
        var customerA = CreateCustomer("52998224725", "John", "john@autogarage.com");
        var vehicle = CreateVehicle(customerA.Id);
        var sut = CreateSut([customerA], vehicle);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(new TransferVehicleOwnerCommand(vehicle.Id.ToString(), ""), CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The newOwnerDocument field is required.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    private static Customer CreateCustomer(string document, string name, string email)
    {
        return Customer.Create(name, new Document(document), email, "41", "999999999");
    }

    private static Vehicle CreateVehicle(Guid customerId)
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, customerId);
    }

    private static TransferVehicleOwnerUseCaseSut CreateSut(Customer[] customers, params Vehicle[] vehicles)
    {
        var customerRepository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            customerRepository.WithCustomer(customer);
        }

        var vehicleRepository = new FakeVehicleRepository();

        foreach (var vehicle in vehicles)
        {
            vehicleRepository.WithVehicle(vehicle);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new TransferVehicleOwnerUseCase(vehicleRepository, vehicleRepository, customerRepository, unitWork, MapperBuilder.Build());

        return new TransferVehicleOwnerUseCaseSut(useCase, unitWork);
    }

    private sealed record TransferVehicleOwnerUseCaseSut(
        TransferVehicleOwnerUseCase UseCase,
        FakeUnitWork UnitWork);
}
