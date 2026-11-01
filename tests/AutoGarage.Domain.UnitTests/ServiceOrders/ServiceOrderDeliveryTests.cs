using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderDeliveryTests
{
    private static readonly Guid Employee = Guid.NewGuid();

    [Fact]
    public void ChangeStatus_FinishedToDelivered_ShouldUpdateStatusAndReturnHistory()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, Employee);
        order.Complete("sem reparo", Employee);

        var history = order.ChangeStatus(ServiceOrderStatus.DELIVERED, Employee);

        order.Status.ShouldBe(ServiceOrderStatus.DELIVERED);
        history.FromStatus.ShouldBe(ServiceOrderStatus.FINISHED);
        history.ToStatus.ShouldBe(ServiceOrderStatus.DELIVERED);
    }

    [Fact]
    public void ChangeStatus_CancelledToDelivered_ShouldUpdateStatusAndReturnHistory()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.Cancel("cliente desistiu", Employee);

        var history = order.ChangeStatus(ServiceOrderStatus.DELIVERED, Employee);

        order.Status.ShouldBe(ServiceOrderStatus.DELIVERED);
        history.FromStatus.ShouldBe(ServiceOrderStatus.CANCELLED);
        history.ToStatus.ShouldBe(ServiceOrderStatus.DELIVERED);
    }
}
