using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CompleteServiceOrder;
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
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.CompleteServiceOrder;

public class CompleteServiceOrderUseCaseTests
{
    [Fact]
    public async Task Handle_InDiagnosis_ShouldFinishWithObservationAndHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Sem reparo", employee.Id.ToString()), CancellationToken.None);

        var stored = sut.Orders.Orders[0];
        stored.Status.ShouldBe(ServiceOrderStatus.FINISHED);
        stored.Observation.ShouldBe("Sem reparo");

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.EventType.ShouldBe(ServiceOrderEventType.STATUS_CHANGE);
        history.FromStatus.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        history.ToStatus.ShouldBe(ServiceOrderStatus.FINISHED);
        history.EmployeeId.ShouldBe(employee.Id);

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_InDiagnosis_ShouldReturnResponseWithFinishedStatus()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var result = await sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Sem reparo", employee.Id.ToString()), CancellationToken.None);

        result.Status.ShouldBe(ServiceOrderStatus.FINISHED);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    public async Task Handle_InDiagnosisOrInExecution_ShouldFinishWithHistoryFromThatStatus(ServiceOrderStatus from)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(from, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Reparo concluído", employee.Id.ToString()), CancellationToken.None);

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.FINISHED);
        sut.Histories.Histories[0].FromStatus.ShouldBe(from);
    }

    [Fact]
    public async Task Handle_ObservationWithSurroundingSpaces_ShouldStoreTrimmedObservation()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        await sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "  Sem reparo  ", employee.Id.ToString()), CancellationToken.None);

        sut.Orders.Orders[0].Observation.ShouldBe("Sem reparo");
    }

    [Fact]
    public async Task Handle_InExecution_ShouldNotChangeBudgetTotal()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_EXECUTION, employee.Id);
        var budgetBefore = order.BudgetTotal;
        var sut = CreateSut(orders: [order], employees: [employee]);

        await sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Reparo concluído", employee.Id.ToString()), CancellationToken.None);

        sut.Orders.Orders[0].BudgetTotal.ShouldBe(budgetBefore);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    public async Task Handle_NotFinishable_ShouldThrowInvalidTransitionAndChangeNothing(ServiceOrderStatus status)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(status, employee.Id);
        var observationBefore = order.Observation;
        var sut = CreateSut(orders: [order], employees: [employee]);

        var exception = await Should.ThrowAsync<InvalidServiceOrderTransitionException>(
            () => sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Sem reparo", employee.Id.ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-transition");
        sut.Orders.Orders[0].Status.ShouldBe(status);
        sut.Orders.Orders[0].Observation.ShouldBe(observationBefore);
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_AlreadyFinished_ShouldNotChangeObservation()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        await sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Original", employee.Id.ToString()), CancellationToken.None);

        await Should.ThrowAsync<InvalidServiceOrderTransitionException>(
            () => sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Nova", employee.Id.ToString()), CancellationToken.None));

        sut.Orders.Orders[0].Observation.ShouldBe("Original");
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveAndKeepStatus()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(new CompleteServiceOrderCommand(order.Id.ToString(), "Sem reparo", employee.Id.ToString()), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-employee-inactive");
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new CompleteServiceOrderCommand(Guid.NewGuid().ToString(), "Sem reparo", employee.Id.ToString());

        var exception = await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-found");
    }

    [Fact]
    public async Task Handle_InvalidOrderId_ShouldThrowInvalidServiceOrderIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new CompleteServiceOrderCommand("abc", "Sem reparo", employee.Id.ToString());

        var exception = await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-id");
    }

    [Fact]
    public async Task Handle_BlankObservation_ShouldThrowErrorOnValidationWithoutCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);
        var command = new CompleteServiceOrderCommand(order.Id.ToString(), "   ", employee.Id.ToString());

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    private static CompleteServiceOrderUseCaseSut CreateSut(ServiceOrder[]? orders = null, Employee[]? employees = null)
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

        var useCase = new CompleteServiceOrderUseCase(lookup, orderRepository, historyRepository, unitWork, MapperBuilder.Build());

        return new CompleteServiceOrderUseCaseSut(useCase, orderRepository, historyRepository, unitWork);
    }

    private sealed record CompleteServiceOrderUseCaseSut(
        CompleteServiceOrderUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
