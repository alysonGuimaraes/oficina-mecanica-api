using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.DiagnoseServiceOrder;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.DiagnoseServiceOrder;

public class DiagnoseServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_ReceivedOrder_ShouldMoveToInDiagnosisWithHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.RECEIVED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.FromStatus.ShouldBe(ServiceOrderStatus.RECEIVED);
        history.ToStatus.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        history.EmployeeId.ShouldBe(employee.Id);

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ReceivedOrder_ShouldReturnResponseWithInDiagnosisStatus()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.RECEIVED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var result = await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        result.Id.ShouldBe(order.Id);
        result.Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    public async Task Handle_OrderNotInReceived_ShouldThrowInvalidServiceOrderTransitionAndChangeNothing(ServiceOrderStatus status)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(status, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var exception = await Should.ThrowAsync<InvalidServiceOrderTransitionException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-transition");
        sut.Orders.Orders[0].Status.ShouldBe(status);
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveAndKeepReceived()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.RECEIVED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.RECEIVED);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new ServiceOrderEmployeeCommand(Guid.NewGuid().ToString(), employee.Id.ToString());

        await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidOrderId_ShouldThrowInvalidServiceOrderIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new ServiceOrderEmployeeCommand("abc", employee.Id.ToString());

        await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnknownEmployee_ShouldThrowEmployeeNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.RECEIVED, employee.Id);
        var sut = CreateSut(orders: [order]);
        var command = new ServiceOrderEmployeeCommand(order.Id.ToString(), Guid.NewGuid().ToString());

        await Should.ThrowAsync<EmployeeNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BlankEmployeeId_ShouldThrowErrorOnValidationExceptionWithoutCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.RECEIVED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new ServiceOrderEmployeeCommand(order.Id.ToString(), "   ");

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    private static ServiceOrderEmployeeCommand ValidCommand(ServiceOrder order, Employee employee) =>
        new(order.Id.ToString(), employee.Id.ToString());

    private static DiagnoseServiceOrderUseCaseSut CreateSut(ServiceOrder[]? orders = null, Employee[]? employees = null)
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

        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();
        var lookup = new ServiceOrderLookup(orderRepository, employeeRepository);

        var useCase = new DiagnoseServiceOrderUseCase(lookup, orderRepository, historyRepository, unitWork, MapperBuilder.Build());

        return new DiagnoseServiceOrderUseCaseSut(useCase, orderRepository, historyRepository, unitWork);
    }

    private sealed record DiagnoseServiceOrderUseCaseSut(
        DiagnoseServiceOrderUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
