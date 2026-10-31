using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderStatusHistoryTests
{
    [Fact]
    public void ForStatusChange_ShouldSetStatusFieldsAndNoItem()
    {
        var serviceOrderId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var history = ServiceOrderStatusHistory.ForStatusChange(serviceOrderId, ServiceOrderStatus.RECEIVED, ServiceOrderStatus.IN_DIAGNOSIS, employeeId);

        history.ServiceOrderId.ShouldBe(serviceOrderId);
        history.EventType.ShouldBe(ServiceOrderEventType.STATUS_CHANGE);
        history.FromStatus.ShouldBe(ServiceOrderStatus.RECEIVED);
        history.ToStatus.ShouldBe(ServiceOrderStatus.IN_DIAGNOSIS);
        history.ItemId.ShouldBeNull();
        history.ItemDirection.ShouldBeNull();
        history.EmployeeId.ShouldBe(employeeId);
        history.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(ServiceOrderEventType.ITEM_SERVICE)]
    [InlineData(ServiceOrderEventType.ITEM_MATERIAL)]
    public void ForItemChange_ShouldSetItemFieldsAndNoStatus(ServiceOrderEventType eventType)
    {
        var itemId = Guid.NewGuid();

        var history = ServiceOrderStatusHistory.ForItemChange(Guid.NewGuid(), eventType, itemId, ServiceOrderItemDirection.IN, Guid.NewGuid());

        history.EventType.ShouldBe(eventType);
        history.ItemId.ShouldBe(itemId);
        history.ItemDirection.ShouldBe(ServiceOrderItemDirection.IN);
        history.FromStatus.ShouldBeNull();
        history.ToStatus.ShouldBeNull();
    }

    [Fact]
    public void ForItemChange_WithStatusEventType_ShouldThrowArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            ServiceOrderStatusHistory.ForItemChange(Guid.NewGuid(), ServiceOrderEventType.STATUS_CHANGE, Guid.NewGuid(), ServiceOrderItemDirection.IN, Guid.NewGuid()));
    }
}
