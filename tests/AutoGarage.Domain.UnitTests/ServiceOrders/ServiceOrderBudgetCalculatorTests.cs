using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Utilities;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderBudgetCalculatorTests
{
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly HashSet<Guid> NoInactiveCatalogServices = new();

    [Fact]
    public void Calculate_ActiveServiceAndMaterials_ShouldSumPriceAndQuantityTimesUnitPrice()
    {
        var services = new List<ServiceOrderService> { ServiceOrderService.Create(OrderId, Guid.NewGuid(), 80m) };
        var materials = new List<ServiceOrderMaterial> { ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 2, 15m) };

        var total = ServiceOrderBudgetCalculator.Calculate(services, materials, NoInactiveCatalogServices);

        total.ShouldBe(110m);
    }

    [Fact]
    public void Calculate_InactiveMaterial_ShouldBeLeftOut()
    {
        var services = new List<ServiceOrderService> { ServiceOrderService.Create(OrderId, Guid.NewGuid(), 80m) };
        var removed = ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 2, 15m);
        removed.Deactivate();

        var total = ServiceOrderBudgetCalculator.Calculate(services, new List<ServiceOrderMaterial> { removed }, NoInactiveCatalogServices);

        total.ShouldBe(80m);
    }

    [Fact]
    public void Calculate_RemovedServiceRowWithAnotherActiveService_ShouldIgnoreTheRemovedRow()
    {
        var removed = ServiceOrderService.Create(OrderId, Guid.NewGuid(), 80m);
        removed.Deactivate();
        var services = new List<ServiceOrderService> { removed, ServiceOrderService.Create(OrderId, Guid.NewGuid(), 50m) };

        var total = ServiceOrderBudgetCalculator.Calculate(services, new List<ServiceOrderMaterial>(), NoInactiveCatalogServices);

        total.ShouldBe(50m);
    }

    [Fact]
    public void Calculate_OnlyActiveMaterialsWithNoService_ShouldThrowWithoutItems()
    {
        var materials = new List<ServiceOrderMaterial> { ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 3, 10m) };

        var exception = Should.Throw<ServiceOrderBudgetWithoutItemsException>(
            () => ServiceOrderBudgetCalculator.Calculate(new List<ServiceOrderService>(), materials, NoInactiveCatalogServices));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
    }

    [Fact]
    public void Calculate_NoActiveItems_ShouldThrowWithoutItems()
    {
        var exception = Should.Throw<ServiceOrderBudgetWithoutItemsException>(
            () => ServiceOrderBudgetCalculator.Calculate(new List<ServiceOrderService>(), new List<ServiceOrderMaterial>(), NoInactiveCatalogServices));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
    }

    [Fact]
    public void Calculate_OnlyRemovedServiceRow_ShouldThrowWithoutItems()
    {
        var removed = ServiceOrderService.Create(OrderId, Guid.NewGuid(), 80m);
        removed.Deactivate();

        var exception = Should.Throw<ServiceOrderBudgetWithoutItemsException>(
            () => ServiceOrderBudgetCalculator.Calculate(new List<ServiceOrderService> { removed }, new List<ServiceOrderMaterial>(), NoInactiveCatalogServices));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
    }

    [Fact]
    public void Calculate_ActiveServiceInactiveInCatalog_ShouldThrowInactiveServiceWithCatalogId()
    {
        var serviceId = Guid.NewGuid();
        var services = new List<ServiceOrderService> { ServiceOrderService.Create(OrderId, serviceId, 80m) };
        var inactiveInCatalog = new HashSet<Guid> { serviceId };

        var exception = Should.Throw<ServiceOrderBudgetInactiveServiceException>(
            () => ServiceOrderBudgetCalculator.Calculate(services, new List<ServiceOrderMaterial>(), inactiveInCatalog));

        exception.ErrorType.ShouldBe("service-order-budget-inactive-service");
        exception.Message.ShouldContain(serviceId.ToString());
    }

    [Fact]
    public void Calculate_InactiveCatalogServiceNotInTheOrder_ShouldNotBlock()
    {
        var services = new List<ServiceOrderService> { ServiceOrderService.Create(OrderId, Guid.NewGuid(), 80m) };
        var inactiveInCatalog = new HashSet<Guid> { Guid.NewGuid() };

        var total = ServiceOrderBudgetCalculator.Calculate(services, new List<ServiceOrderMaterial>(), inactiveInCatalog);

        total.ShouldBe(80m);
    }

    [Fact]
    public void Calculate_RemovedRowWhoseCatalogServiceIsInactive_ShouldNotBlock()
    {
        var serviceId = Guid.NewGuid();
        var removed = ServiceOrderService.Create(OrderId, serviceId, 80m);
        removed.Deactivate();
        var services = new List<ServiceOrderService> { removed, ServiceOrderService.Create(OrderId, Guid.NewGuid(), 50m) };
        var inactiveInCatalog = new HashSet<Guid> { serviceId };

        var total = ServiceOrderBudgetCalculator.Calculate(services, new List<ServiceOrderMaterial>(), inactiveInCatalog);

        total.ShouldBe(50m);
    }
}
