using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.ServiceOrders;

public class ServiceOrderLookupTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    public async Task GetOrder_InvalidId_ShouldThrowInvalidServiceOrderIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.GetOrder(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-id");
    }

    [Fact]
    public async Task GetOrder_UnknownId_ShouldThrowServiceOrderNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.GetOrder(Guid.NewGuid().ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-found");
    }

    [Fact]
    public async Task GetOrder_ExistingId_ShouldReturnOrder()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var sut = CreateSut(orders: [order]);

        var result = await sut.GetOrder(order.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(order.Id);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    public async Task EnsureActiveEmployee_InvalidId_ShouldThrowInvalidEmployeeIdException(string employeeId)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidEmployeeIdException>(
            () => sut.EnsureActiveEmployee(employeeId, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-employee-id");
    }

    [Fact]
    public async Task EnsureActiveEmployee_UnknownId_ShouldThrowEmployeeNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<EmployeeNotFoundException>(
            () => sut.EnsureActiveEmployee(Guid.NewGuid().ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("employee-not-found");
    }

    [Fact]
    public async Task EnsureActiveEmployee_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var sut = CreateSut(employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.EnsureActiveEmployee(employee.Id.ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-employee-inactive");
    }

    [Fact]
    public async Task EnsureActiveEmployee_ActiveEmployee_ShouldReturnItsId()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);

        var result = await sut.EnsureActiveEmployee(employee.Id.ToString(), CancellationToken.None);

        result.ShouldBe(employee.Id);
    }

    private static ServiceOrderLookup CreateSut(ServiceOrder[]? orders = null, Employee[]? employees = null)
    {
        var orderRepository = new FakeServiceOrderRepository();
        foreach (var order in orders ?? [])
        {
            orderRepository.Add(order, CancellationToken.None);
        }

        var employeeRepository = new FakeEmployeeRepository();
        foreach (var employee in employees ?? [])
        {
            employeeRepository.WithEmployee(employee);
        }

        return new ServiceOrderLookup(orderRepository, employeeRepository);
    }
}
