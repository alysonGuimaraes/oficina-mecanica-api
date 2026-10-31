using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CreateServiceOrder;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.CommonTestUtilities.Repositories.Vehicles;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.CreateServiceOrder;

public class CreateServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_ValidInput_ShouldCreateReceivedOrderWithInitialHistoryAndCommit()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [customer], vehicles: [vehicle], employees: [employee]);

        await sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None);

        sut.Orders.Orders.Count.ShouldBe(1);
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.RECEIVED);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.FromStatus.ShouldBeNull();
        history.ToStatus.ShouldBe(ServiceOrderStatus.RECEIVED);
        history.EmployeeId.ShouldBe(employee.Id);
        history.ServiceOrderId.ShouldBe(sut.Orders.Orders[0].Id);

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ValidInput_ShouldReturnResponseWithReceivedStatusAndNoBudget()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [customer], vehicles: [vehicle], employees: [employee]);

        var result = await sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None);

        result.Status.ShouldBe(ServiceOrderStatus.RECEIVED);
        result.BudgetTotal.ShouldBeNull();
        result.CustomerId.ShouldBe(customer.Id);
        result.VehicleId.ShouldBe(vehicle.Id);
    }

    [Fact]
    public async Task Handle_InvalidCustomerId_ShouldThrowInvalidCustomerIdException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidCustomerIdException>(() =>
            sut.UseCase.Handle(new CreateServiceOrderCommand("abc", Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-id");
        sut.Orders.Orders.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidVehicleId_ShouldThrowInvalidVehicleIdException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidVehicleIdException>(() =>
            sut.UseCase.Handle(new CreateServiceOrderCommand(Guid.NewGuid().ToString(), "123", Guid.NewGuid().ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-vehicle-id");
        sut.Orders.Orders.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_EmptyCustomerId_ShouldThrowErrorOnValidation()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() =>
            sut.UseCase.Handle(new CreateServiceOrderCommand("", Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The customerId field is required.");
    }

    [Fact]
    public async Task Handle_InvalidEmployeeId_ShouldThrowInvalidEmployeeIdException()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var sut = CreateSut(customers: [customer], vehicles: [vehicle]);

        var exception = await Should.ThrowAsync<InvalidEmployeeIdException>(() =>
            sut.UseCase.Handle(new CreateServiceOrderCommand(customer.Id.ToString(), vehicle.Id.ToString(), "xyz"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-employee-id");
    }

    [Fact]
    public async Task Handle_UnknownCustomer_ShouldThrowCustomerNotFoundException()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(vehicles: [vehicle], employees: [employee]);

        var exception = await Should.ThrowAsync<CustomerNotFoundException>(() =>
            sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-not-found");
    }

    [Fact]
    public async Task Handle_InactiveCustomer_ShouldThrowServiceOrderCustomerInactive()
    {
        var customer = CreateCustomer();
        customer.Deactivate();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [customer], vehicles: [vehicle], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderCustomerInactiveException>(() =>
            sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-customer-inactive");
        sut.Orders.Orders.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownVehicle_ShouldThrowVehicleNotFoundException()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [customer], employees: [employee]);

        var exception = await Should.ThrowAsync<VehicleNotFoundException>(() =>
            sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("vehicle-not-found");
    }

    [Fact]
    public async Task Handle_InactiveVehicle_ShouldThrowServiceOrderVehicleInactive()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        vehicle.Deactivate();
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [customer], vehicles: [vehicle], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderVehicleInactiveException>(() =>
            sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-vehicle-inactive");
    }

    [Fact]
    public async Task Handle_VehicleOfAnotherCustomer_ShouldThrowServiceOrderVehicleCustomerMismatch()
    {
        var owner = CreateCustomer();
        var otherCustomer = CreateCustomer();
        var vehicle = CreateVehicle(owner);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [owner, otherCustomer], vehicles: [vehicle], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderVehicleCustomerMismatchException>(() =>
            sut.UseCase.Handle(ValidCommand(otherCustomer, vehicle, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-vehicle-customer-mismatch");
        sut.Orders.Orders.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownEmployee_ShouldThrowEmployeeNotFoundException()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [customer], vehicles: [vehicle]);

        var exception = await Should.ThrowAsync<EmployeeNotFoundException>(() =>
            sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-not-found");
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactive()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var sut = CreateSut(customers: [customer], vehicles: [vehicle], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(() =>
            sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-employee-inactive");
    }

    [Fact]
    public async Task Handle_EmployeeIdOfAnotherEntity_ShouldThrowEmployeeNotFound()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(customers: [customer], vehicles: [vehicle], employees: [employee]);

        var exception = await Should.ThrowAsync<EmployeeNotFoundException>(() =>
            sut.UseCase.Handle(new CreateServiceOrderCommand(customer.Id.ToString(), vehicle.Id.ToString(), customer.Id.ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-not-found");
    }

    [Fact]
    public async Task Handle_RejectedInput_ShouldNotWriteAnything()
    {
        var customer = CreateCustomer();
        var vehicle = CreateVehicle(customer);
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var sut = CreateSut(customers: [customer], vehicles: [vehicle], employees: [employee]);

        await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(() =>
            sut.UseCase.Handle(ValidCommand(customer, vehicle, employee), CancellationToken.None));

        sut.Orders.Orders.Count.ShouldBe(0);
        sut.Histories.Histories.Count.ShouldBe(0);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    private static CreateServiceOrderCommand ValidCommand(Customer customer, Vehicle vehicle, Employee employee)
    {
        return new CreateServiceOrderCommand(customer.Id.ToString(), vehicle.Id.ToString(), employee.Id.ToString());
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create("John", new Document("39053344705"), "john@autogarage.com", "41", "999999999");
    }

    private static Vehicle CreateVehicle(Customer owner)
    {
        return Vehicle.Create("GOL", "1.0", new Plate("ABC1D23"), 2020, VehicleType.CAR, owner.Id);
    }

    private static CreateServiceOrderUseCaseSut CreateSut(
        Customer[]? customers = null,
        Vehicle[]? vehicles = null,
        Employee[]? employees = null)
    {
        var customerRepository = new FakeCustomerRepository();
        foreach (var customer in customers ?? [])
        {
            customerRepository.WithCustomer(customer);
        }

        var vehicleRepository = new FakeVehicleRepository();
        foreach (var vehicle in vehicles ?? [])
        {
            vehicleRepository.WithVehicle(vehicle);
        }

        var employeeRepository = new FakeEmployeeRepository();
        foreach (var employee in employees ?? [])
        {
            employeeRepository.WithEmployee(employee);
        }

        var orderRepository = new FakeServiceOrderRepository();
        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();

        var useCase = new CreateServiceOrderUseCase(
            customerRepository,
            vehicleRepository,
            employeeRepository,
            orderRepository,
            historyRepository,
            unitWork,
            MapperBuilder.Build());

        return new CreateServiceOrderUseCaseSut(useCase, orderRepository, historyRepository, unitWork);
    }

    private sealed record CreateServiceOrderUseCaseSut(
        CreateServiceOrderUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
