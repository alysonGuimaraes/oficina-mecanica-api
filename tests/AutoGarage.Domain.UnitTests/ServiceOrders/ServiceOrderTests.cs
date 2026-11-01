using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderTests
{
    private static readonly Guid Employee = Guid.NewGuid();

    [Fact]
    public void Create_ShouldStartInReceivedWithUtcCreatedAt()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var order = ServiceOrder.Create(customerId, vehicleId);

        order.Status.ShouldBe(ServiceOrderStatus.RECEIVED);
        order.CustomerId.ShouldBe(customerId);
        order.VehicleId.ShouldBe(vehicleId);
        order.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        order.UpdatedAt.ShouldBeNull();
        order.BudgetTotal.ShouldBeNull();
    }

    [Fact]
    public void ChangeStatus_AllowedTarget_ShouldUpdateStatusAndReturnHistory()
    {
        var order = OrderAt(ServiceOrderStatus.RECEIVED);

        var history = order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);

        order.Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        order.UpdatedAt.ShouldNotBeNull();
        history.EventType.ShouldBe(ServiceOrderEventType.STATUS_CHANGE);
        history.FromStatus.ShouldBe(ServiceOrderStatus.RECEIVED);
        history.ToStatus.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        history.EmployeeId.ShouldBe(Employee);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    public void ChangeStatus_TargetWithOwnMethod_ShouldThrowAndKeepStatus(ServiceOrderStatus target)
    {
        var order = OrderAt(ServiceOrderStatus.IN_DIAGNOSIS);

        Should.Throw<InvalidServiceOrderTransitionException>(() => order.ChangeStatus(target, Employee));

        order.Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Fact]
    public void ChangeStatus_DisallowedTransition_ShouldThrowAndKeepStatus()
    {
        var order = OrderAt(ServiceOrderStatus.RECEIVED);

        Should.Throw<InvalidServiceOrderTransitionException>(() => order.ChangeStatus(ServiceOrderStatus.DELIVERED, Employee));

        order.Status.ShouldBe(ServiceOrderStatus.RECEIVED);
    }

    [Fact]
    public void SendBudget_FromInDiagnosis_ShouldSetBudgetAndSentAt()
    {
        var order = OrderAt(ServiceOrderStatus.IN_DIAGNOSIS);

        var history = order.SendBudget(150.00m, Employee);

        order.Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
        order.BudgetTotal.ShouldBe(150.00m);
        order.BudgetSentAt.ShouldNotBeNull();
        history.ToStatus.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
    }

    [Fact]
    public void SendBudget_FromOtherStatus_ShouldThrowAndKeepBudgetEmpty()
    {
        var order = OrderAt(ServiceOrderStatus.RECEIVED);

        Should.Throw<InvalidServiceOrderTransitionException>(() => order.SendBudget(150.00m, Employee));

        order.BudgetTotal.ShouldBeNull();
    }

    [Theory]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    public void Complete_FromInDiagnosisOrInExecution_ShouldFinishWithObservation(ServiceOrderStatus from)
    {
        var order = OrderAt(from);

        order.Complete("Reparo concluído", Employee);

        order.Status.ShouldBe(ServiceOrderStatus.FINISHED);
        order.Observation.ShouldBe("Reparo concluído");
    }

    [Fact]
    public void Complete_FromOtherStatus_ShouldThrow()
    {
        var order = OrderAt(ServiceOrderStatus.AWAITING_APPROVAL);

        Should.Throw<InvalidServiceOrderTransitionException>(() => order.Complete("x", Employee));

        order.Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
    }

    [Fact]
    public void Refuse_FromAwaitingApproval_ShouldFinishWithoutReason()
    {
        var order = OrderAt(ServiceOrderStatus.AWAITING_APPROVAL);

        order.Refuse(Employee);

        order.Status.ShouldBe(ServiceOrderStatus.FINISHED);
        order.Observation.ShouldBeNull();
        order.CancellationReason.ShouldBeNull();
    }

    [Fact]
    public void Refuse_FromOtherStatus_ShouldThrow()
    {
        var order = OrderAt(ServiceOrderStatus.IN_DIAGNOSIS);

        Should.Throw<InvalidServiceOrderTransitionException>(() => order.Refuse(Employee));

        order.Status.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    public void Cancel_FromAllowedStates_ShouldCancelWithReason(ServiceOrderStatus from)
    {
        var order = OrderAt(from);

        order.Cancel("Cliente desistiu", Employee);

        order.Status.ShouldBe(ServiceOrderStatus.CANCELLED);
        order.CancellationReason.ShouldBe("Cliente desistiu");
    }

    [Theory]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    public void Cancel_FromFinishedDeliveredOrCancelled_ShouldThrow(ServiceOrderStatus from)
    {
        var order = OrderAt(from);

        Should.Throw<InvalidServiceOrderTransitionException>(() => order.Cancel("x", Employee));

        order.Status.ShouldBe(from);
    }

    [Fact]
    public void EnsureItemsEditable_InDiagnosis_ShouldNotThrow()
    {
        var order = OrderAt(ServiceOrderStatus.IN_DIAGNOSIS);

        Should.NotThrow(() => order.EnsureItemsEditable());
    }

    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED)]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL)]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS)]
    [InlineData(ServiceOrderStatus.IN_EXECUTION)]
    [InlineData(ServiceOrderStatus.FINISHED)]
    [InlineData(ServiceOrderStatus.CANCELLED)]
    [InlineData(ServiceOrderStatus.DELIVERED)]
    public void EnsureItemsEditable_OtherStatus_ShouldThrowServiceOrderNotEditable(ServiceOrderStatus status)
    {
        var order = OrderAt(status);

        var exception = Should.Throw<ServiceOrderNotEditableException>(() => order.EnsureItemsEditable());

        exception.ErrorType.ShouldBe("service-order-not-editable");
    }

    // Leva a OS até o estado pedido, sempre pelo caminho válido da matriz.
    private static ServiceOrder OrderAt(ServiceOrderStatus status)
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        switch (status)
        {
            case ServiceOrderStatus.RECEIVED:
                break;
            case ServiceOrderStatus.IN_DIAGNOSIS:
                order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);
                break;
            case ServiceOrderStatus.AWAITING_APPROVAL:
                order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);
                order.SendBudget(100.00m, Employee);
                break;
            case ServiceOrderStatus.AWAITING_MATERIALS:
                order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);
                order.SendBudget(100.00m, Employee);
                order.ChangeStatus(ServiceOrderStatus.AWAITING_MATERIALS, Employee);
                break;
            case ServiceOrderStatus.IN_EXECUTION:
                order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);
                order.SendBudget(100.00m, Employee);
                order.ChangeStatus(ServiceOrderStatus.IN_EXECUTION, Employee);
                break;
            case ServiceOrderStatus.FINISHED:
                order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);
                order.Complete("Encerrada", Employee);
                break;
            case ServiceOrderStatus.DELIVERED:
                order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);
                order.Complete("Encerrada", Employee);
                order.ChangeStatus(ServiceOrderStatus.DELIVERED, Employee);
                break;
            case ServiceOrderStatus.CANCELLED:
                order.Cancel("Cancelada", Employee);
                break;
        }

        return order;
    }
}
