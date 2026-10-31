using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.DeliverServiceOrder;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.DeliverServiceOrder;

public class DeliverServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_FinishedOrder_ShouldDeliverWithHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.FINISHED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.DELIVERED);

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.FromStatus.ShouldBe(ServiceOrderStatus.FINISHED);
        history.ToStatus.ShouldBe(ServiceOrderStatus.DELIVERED);
        history.EmployeeId.ShouldBe(employee.Id);

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_CancelledOrder_ShouldDeliver()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.CANCELLED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var result = await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        result.Status.ShouldBe(ServiceOrderStatus.DELIVERED);
        sut.Histories.Histories[0].FromStatus.ShouldBe(ServiceOrderStatus.CANCELLED);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    public async Task Handle_NotDeliverableStatus_ShouldThrowInvalidServiceOrderTransitionAndChangeNothing(ServiceOrderStatus status)
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
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveAndKeepFinished()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.FINISHED, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.FINISHED);
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

    private static ServiceOrderEmployeeCommand ValidCommand(ServiceOrder order, Employee employee) =>
        new(order.Id.ToString(), employee.Id.ToString());

    private static DeliverServiceOrderUseCaseSut CreateSut(ServiceOrder[]? orders = null, Employee[]? employees = null)
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

        var useCase = new DeliverServiceOrderUseCase(lookup, orderRepository, historyRepository, unitWork, MapperBuilder.Build());

        return new DeliverServiceOrderUseCaseSut(useCase, orderRepository, historyRepository, unitWork);
    }

    private sealed record DeliverServiceOrderUseCaseSut(
        DeliverServiceOrderUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
