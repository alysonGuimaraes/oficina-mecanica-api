using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Utilities;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderBudgetCalculatorTests
{
    private static readonly Guid OrderId = Guid.NewGuid();

    [Fact]
    public void Calculate_ActiveServiceAndMaterials_ShouldSumPriceAndQuantityTimesUnitPrice()
    {
        var services = new List<ServiceOrderService> { ServiceOrderService.Create(OrderId, Guid.NewGuid(), 80m) };
        var materials = new List<ServiceOrderMaterial> { ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 2, 15m) };

        var total = ServiceOrderBudgetCalculator.Calculate(services, materials);

        total.ShouldBe(110m);
    }

    [Fact]
    public void Calculate_InactiveMaterial_ShouldBeLeftOut()
    {
        var services = new List<ServiceOrderService> { ServiceOrderService.Create(OrderId, Guid.NewGuid(), 80m) };
        var removed = ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 2, 15m);
        removed.Deactivate();
        var materials = new List<ServiceOrderMaterial> { removed };

        var total = ServiceOrderBudgetCalculator.Calculate(services, materials);

        total.ShouldBe(80m);
    }

    [Fact]
    public void Calculate_OnlyActiveMaterials_ShouldSumMaterials()
    {
        var services = new List<ServiceOrderService>();
        var materials = new List<ServiceOrderMaterial> { ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 3, 10m) };

        var total = ServiceOrderBudgetCalculator.Calculate(services, materials);

        total.ShouldBe(30m);
    }

    [Fact]
    public void Calculate_NoActiveItems_ShouldThrowWithoutItems()
    {
        var exception = Should.Throw<ServiceOrderBudgetWithoutItemsException>(
            () => ServiceOrderBudgetCalculator.Calculate(new List<ServiceOrderService>(), new List<ServiceOrderMaterial>()));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
    }

    [Fact]
    public void Calculate_OnlyInactiveMaterials_ShouldThrowWithoutItems()
    {
        var removed = ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 2, 15m);
        removed.Deactivate();

        var exception = Should.Throw<ServiceOrderBudgetWithoutItemsException>(
            () => ServiceOrderBudgetCalculator.Calculate(new List<ServiceOrderService>(), new List<ServiceOrderMaterial> { removed }));

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
    }

    [Fact]
    public void Calculate_InactiveService_ShouldThrowInactiveServiceEvenWithActiveMaterial()
    {
        var serviceId = Guid.NewGuid();
        var service = ServiceOrderService.Create(OrderId, serviceId, 80m);
        service.Deactivate();
        var materials = new List<ServiceOrderMaterial> { ServiceOrderMaterial.Create(OrderId, Guid.NewGuid(), 1, 10m) };

        var exception = Should.Throw<ServiceOrderBudgetInactiveServiceException>(
            () => ServiceOrderBudgetCalculator.Calculate(new List<ServiceOrderService> { service }, materials));

        exception.ErrorType.ShouldBe("service-order-budget-inactive-service");
        exception.Message.ShouldContain(serviceId.ToString());
    }
}
