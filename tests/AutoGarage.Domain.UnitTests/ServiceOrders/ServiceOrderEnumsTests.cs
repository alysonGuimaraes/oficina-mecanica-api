using AutoGarage.Domain.Enums.ServiceOrders;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderEnumsTests
{
    [Fact]
    public void ServiceOrderStatus_ShouldHaveSpecifiedValues()
    {
        ((int)ServiceOrderStatus.RECEIVED).ShouldBe(1);
        ((int)ServiceOrderStatus.IN_DIAGNOSIS).ShouldBe(2);
        ((int)ServiceOrderStatus.AWAITING_APPROVAL).ShouldBe(3);
        ((int)ServiceOrderStatus.AWAITING_MATERIALS).ShouldBe(4);
        ((int)ServiceOrderStatus.IN_EXECUTION).ShouldBe(5);
        ((int)ServiceOrderStatus.FINISHED).ShouldBe(6);
        ((int)ServiceOrderStatus.CANCELLED).ShouldBe(7);
        ((int)ServiceOrderStatus.DELIVERED).ShouldBe(8);
    }

    [Fact]
    public void ServiceOrderEventType_ShouldHaveSpecifiedValues()
    {
        ((int)ServiceOrderEventType.STATUS_CHANGE).ShouldBe(1);
        ((int)ServiceOrderEventType.ITEM_SERVICE).ShouldBe(2);
        ((int)ServiceOrderEventType.ITEM_MATERIAL).ShouldBe(3);
    }

    [Fact]
    public void ServiceOrderItemDirection_ShouldHaveSpecifiedValues()
    {
        ((int)ServiceOrderItemDirection.IN).ShouldBe(1);
        ((int)ServiceOrderItemDirection.OUT).ShouldBe(2);
    }
}
