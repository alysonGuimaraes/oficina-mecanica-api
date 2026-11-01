using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.SendServiceOrderBudget;
using AutoGarage.CommonTestUtilities.Entities;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Employees;
using AutoGarage.CommonTestUtilities.Repositories.Services;
using AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.SendServiceOrderBudget;

public class SendServiceOrderBudgetUseCaseTests
{
    [Fact]
    public async Task Handle_ItemsInDiagnosis_ShouldSendBudgetWithTotalAndHistoryAndCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee], services: [ActiveService(order)], materials: [ActiveMaterial(order, 6, 12.00m)]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        var stored = sut.Orders.Orders[0];
        stored.Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
        stored.BudgetTotal.ShouldBe(222.50m);
        stored.BudgetSentAt.ShouldNotBeNull();

        sut.Histories.Histories.Count.ShouldBe(1);
        var history = sut.Histories.Histories[0];
        history.EventType.ShouldBe(ServiceOrderEventType.STATUS_CHANGE);
        history.FromStatus.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        history.ToStatus.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
        history.EmployeeId.ShouldBe(employee.Id);

        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ItemsInDiagnosis_ShouldReturnResponseWithBudgetTotal()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee], services: [ActiveService(order)], materials: [ActiveMaterial(order, 6, 12.00m)]);

        var result = await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        result.Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
        result.BudgetTotal.ShouldBe(222.50m);
    }

    [Fact]
    public async Task Handle_RemovedMaterialOnly_ShouldLeaveItOutOfTotal()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var removed = ActiveMaterial(order, 6, 12.00m);
        removed.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], services: [ActiveService(order)], materials: [removed]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        sut.Orders.Orders[0].BudgetTotal.ShouldBe(150.50m);
    }

    [Fact]
    public async Task Handle_RemovedServiceRowWithAnotherActiveService_ShouldIgnoreTheRemovedRow()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var removed = ServiceOrderService.Create(order.Id, Guid.NewGuid(), 80m);
        removed.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], services: [removed, ActiveService(order)]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
        sut.Orders.Orders[0].BudgetTotal.ShouldBe(150.50m);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    public async Task Handle_OrderNotInDiagnosis_ShouldThrowInvalidTransitionAndChangeNothing(ServiceOrderStatus status)
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(status, employee.Id);
        var before = order.BudgetTotal;
        var sut = CreateSut(orders: [order], employees: [employee], services: [ActiveService(order)]);

        var exception = await Should.ThrowAsync<InvalidServiceOrderTransitionException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-transition");
        sut.Orders.Orders[0].Status.ShouldBe(status);
        sut.Orders.Orders[0].BudgetTotal.ShouldBe(before);
        sut.Histories.Histories.ShouldBeEmpty();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_AlreadySentBudget_ShouldNotChangeTotal()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.AWAITING_APPROVAL, employee.Id);
        var sentTotal = order.BudgetTotal;
        var sut = CreateSut(orders: [order], employees: [employee], services: [ActiveService(order)], materials: [ActiveMaterial(order, 6, 12.00m)]);

        await Should.ThrowAsync<InvalidServiceOrderTransitionException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        sut.Orders.Orders[0].BudgetTotal.ShouldBe(sentTotal);
    }

    [Fact]
    public async Task Handle_ActiveServiceInactiveInCatalog_ShouldThrowInactiveServiceAndChangeNothing()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var catalogService = Service.Create("Troca de óleo", null, 150.50m);
        catalogService.Deactivate();
        var row = ServiceOrderService.Create(order.Id, catalogService.Id, 150.50m);
        var sut = CreateSut(orders: [order], employees: [employee], services: [row], catalog: [catalogService], materials: [ActiveMaterial(order, 6, 12.00m)]);

        var exception = await Should.ThrowAsync<ServiceOrderBudgetInactiveServiceException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-budget-inactive-service");
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_RemovedRowWhoseCatalogServiceIsInactive_ShouldNotBlock()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var catalogService = Service.Create("Troca de óleo", null, 80m);
        catalogService.Deactivate();
        var removed = ServiceOrderService.Create(order.Id, catalogService.Id, 80m);
        removed.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], services: [removed, ActiveService(order)], catalog: [catalogService]);

        await sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None);

        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
        sut.Orders.Orders[0].BudgetTotal.ShouldBe(150.50m);
    }

    [Fact]
    public async Task Handle_NoActiveItems_ShouldThrowWithoutItemsAndChangeNothing()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee]);

        var exception = await Should.ThrowAsync<ServiceOrderBudgetWithoutItemsException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_OnlyRemovedServiceRow_ShouldThrowWithoutItems()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var removed = ActiveService(order);
        removed.Deactivate();
        var sut = CreateSut(orders: [order], employees: [employee], services: [removed]);

        var exception = await Should.ThrowAsync<ServiceOrderBudgetWithoutItemsException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
    }

    [Fact]
    public async Task Handle_OnlyActiveMaterials_ShouldThrowWithoutItemsAndChangeNothing()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee], materials: [ActiveMaterial(order, 6, 12.00m)]);

        var exception = await Should.ThrowAsync<ServiceOrderBudgetWithoutItemsException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ShouldThrowServiceOrderEmployeeInactiveAndKeepInDiagnosis()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        employee.Deactivate();
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee], services: [ActiveService(order)]);

        var exception = await Should.ThrowAsync<ServiceOrderEmployeeInactiveException>(
            () => sut.UseCase.Handle(ValidCommand(order, employee), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-employee-inactive");
        sut.Orders.Orders[0].Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ShouldThrowServiceOrderNotFoundException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new ServiceOrderEmployeeCommand(Guid.NewGuid().ToString(), employee.Id.ToString());

        var exception = await Should.ThrowAsync<ServiceOrderNotFoundException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("service-order-not-found");
    }

    [Fact]
    public async Task Handle_InvalidOrderId_ShouldThrowInvalidServiceOrderIdException()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var sut = CreateSut(employees: [employee]);
        var command = new ServiceOrderEmployeeCommand("abc", employee.Id.ToString());

        var exception = await Should.ThrowAsync<InvalidServiceOrderIdException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-order-id");
    }

    [Fact]
    public async Task Handle_BlankEmployeeId_ShouldThrowErrorOnValidationExceptionWithoutCommit()
    {
        var employee = EmployeeBuilder.Build("ana@autogarage.com");
        var order = ServiceOrderBuilder.InStatus(ServiceOrderStatus.IN_DIAGNOSIS, employee.Id);
        var sut = CreateSut(orders: [order], employees: [employee], services: [ActiveService(order)]);
        var command = new ServiceOrderEmployeeCommand(order.Id.ToString(), "   ");

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    private static ServiceOrderService ActiveService(ServiceOrder order) =>
        ServiceOrderService.Create(order.Id, Guid.NewGuid(), 150.50m);

    private static ServiceOrderMaterial ActiveMaterial(ServiceOrder order, int quantity, decimal unitPrice) =>
        ServiceOrderMaterial.Create(order.Id, Guid.NewGuid(), quantity, unitPrice);

    private static ServiceOrderEmployeeCommand ValidCommand(ServiceOrder order, Employee employee) =>
        new(order.Id.ToString(), employee.Id.ToString());

    private static SendServiceOrderBudgetUseCaseSut CreateSut(
        ServiceOrder[]? orders = null,
        Employee[]? employees = null,
        ServiceOrderService[]? services = null,
        ServiceOrderMaterial[]? materials = null,
        Service[]? catalog = null)
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

        var catalogRepository = new FakeServiceRepository();
        foreach (var service in catalog ?? [])
        {
            catalogRepository.WithService(service);
        }

        var serviceRepository = new FakeServiceOrderServiceRepository();
        foreach (var service in services ?? [])
        {
            serviceRepository.Add(service, CancellationToken.None);
        }

        var materialRepository = new FakeServiceOrderMaterialRepository();
        foreach (var material in materials ?? [])
        {
            materialRepository.Add(material, CancellationToken.None);
        }

        var historyRepository = new FakeServiceOrderStatusHistoryRepository();
        var unitWork = new FakeUnitWork();
        var lookup = new ServiceOrderLookup(orderRepository, employeeRepository);

        var useCase = new SendServiceOrderBudgetUseCase(
            lookup, catalogRepository, serviceRepository, materialRepository, orderRepository, historyRepository, unitWork, MapperBuilder.Build());

        return new SendServiceOrderBudgetUseCaseSut(useCase, orderRepository, historyRepository, unitWork);
    }

    private sealed record SendServiceOrderBudgetUseCaseSut(
        SendServiceOrderBudgetUseCase UseCase,
        FakeServiceOrderRepository Orders,
        FakeServiceOrderStatusHistoryRepository Histories,
        FakeUnitWork UnitWork);
}
