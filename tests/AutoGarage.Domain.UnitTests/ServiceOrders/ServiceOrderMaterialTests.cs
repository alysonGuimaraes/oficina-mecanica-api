using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderMaterialTests
{
    [Fact]
    public void Create_ShouldBeActiveWithQuantityAndUnitPrice()
    {
        var serviceOrderId = Guid.NewGuid();
        var materialId = Guid.NewGuid();

        var item = ServiceOrderMaterial.Create(serviceOrderId, materialId, 3, 2.50m);

        item.ServiceOrderId.ShouldBe(serviceOrderId);
        item.MaterialId.ShouldBe(materialId);
        item.Quantity.ShouldBe(3);
        item.UnitPrice.ShouldBe(2.50m);
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        item.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        item.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void AddQuantity_Active_ShouldSumAndSetUpdatedAt()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);

        item.AddQuantity(2);

        item.Quantity.ShouldBe(5);
        item.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void AddQuantity_Inactive_ShouldThrowNotActive()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);
        item.Deactivate();

        var exception = Should.Throw<ServiceOrderItemNotActiveException>(() => item.AddQuantity(2));

        exception.ErrorType.ShouldBe("service-order-item-not-active");
        item.Quantity.ShouldBe(3);
    }

    [Fact]
    public void Deactivate_Active_ShouldBeInactive()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);

        item.Deactivate();

        item.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public void Deactivate_Inactive_ShouldThrowNotActive()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);
        item.Deactivate();

        Should.Throw<ServiceOrderItemNotActiveException>(() => item.Deactivate());
    }

    [Fact]
    public void Reactivate_Inactive_ShouldReplaceQuantityAndUnitPrice()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);
        var id = item.Id;
        item.Deactivate();

        item.Reactivate(4, 3.00m);

        item.Id.ShouldBe(id);
        item.Status.ShouldBe(EntityStatus.ACTIVE);
        item.Quantity.ShouldBe(4);
        item.UnitPrice.ShouldBe(3.00m);
        item.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Reactivate_Active_ShouldThrowAlreadyActive()
    {
        var item = ServiceOrderMaterial.Create(Guid.NewGuid(), Guid.NewGuid(), 3, 2.50m);

        var exception = Should.Throw<ServiceOrderItemAlreadyActiveException>(() => item.Reactivate(4, 3.00m));

        exception.ErrorType.ShouldBe("service-order-item-already-active");
    }
}
