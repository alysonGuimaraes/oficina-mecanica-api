using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderMaterialExceptionTests
{
    [Fact]
    public void ServiceOrderMaterialInactive_ShouldCarryErrorTypeAndMaterialId()
    {
        var materialId = Guid.NewGuid();

        var exception = new ServiceOrderMaterialInactiveException(materialId);

        exception.ErrorType.ShouldBe("service-order-material-inactive");
        exception.Message.ShouldContain(materialId.ToString());
    }
}
