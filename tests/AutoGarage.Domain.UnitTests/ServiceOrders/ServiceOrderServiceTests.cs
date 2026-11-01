using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderServiceTests
{
    [Fact]
    public void Create_ShouldBeActiveWithPriceAndCreatedAt()
    {
        var serviceOrderId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var item = ServiceOrderService.Create(serviceOrderId, serviceId, 80.00m);

        item.ServiceOrderId.ShouldBe(serviceOrderId);
        item.ServiceId.ShouldBe(serviceId);
        item.Price.ShouldBe(80.00m);
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        item.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        item.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Deactivate_Active_ShouldBeInactiveAndSetUpdatedAt()
    {
        var item = ServiceOrderService.Create(Guid.NewGuid(), Guid.NewGuid(), 80.00m);

        item.Deactivate();

        item.Status.ShouldBe(EntityStatus.INACTIVE);
        item.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Deactivate_Inactive_ShouldThrowNotActive()
    {
        var item = ServiceOrderService.Create(Guid.NewGuid(), Guid.NewGuid(), 80.00m);
        item.Deactivate();

        var exception = Should.Throw<ServiceOrderItemNotActiveException>(() => item.Deactivate());

        exception.ErrorType.ShouldBe("service-order-item-not-active");
    }

    [Fact]
    public void Reactivate_Inactive_ShouldBeActiveKeepingIdAndTakingNewPrice()
    {
        var item = ServiceOrderService.Create(Guid.NewGuid(), Guid.NewGuid(), 80.00m);
        var id = item.Id;
        item.Deactivate();

        item.Reactivate(95.00m);

        item.Id.ShouldBe(id);
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        item.Price.ShouldBe(95.00m);
        item.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Reactivate_Active_ShouldThrowAlreadyActive()
    {
        var item = ServiceOrderService.Create(Guid.NewGuid(), Guid.NewGuid(), 80.00m);

        var exception = Should.Throw<ServiceOrderItemAlreadyActiveException>(() => item.Reactivate(95.00m));

        exception.ErrorType.ShouldBe("service-order-item-already-active");
    }
}
