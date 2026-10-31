using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderItemExceptionsTests
{
    [Fact]
    public void InvalidServiceOrderItemId_ShouldCarryErrorType()
    {
        var exception = new InvalidServiceOrderItemIdException();

        exception.ErrorType.ShouldBe("invalid-service-order-item-id");
    }

    [Fact]
    public void ServiceOrderItemNotFound_ShouldCarryErrorTypeAndId()
    {
        var id = Guid.NewGuid();

        var exception = new ServiceOrderItemNotFoundException(id);

        exception.ErrorType.ShouldBe("service-order-item-not-found");
        exception.Message.ShouldContain(id.ToString());
    }

    [Fact]
    public void ServiceOrderServiceInactive_ShouldCarryErrorTypeAndServiceId()
    {
        var serviceId = Guid.NewGuid();

        var exception = new ServiceOrderServiceInactiveException(serviceId);

        exception.ErrorType.ShouldBe("service-order-service-inactive");
        exception.Message.ShouldContain(serviceId.ToString());
    }

    [Fact]
    public void ServiceAlreadyAddedToServiceOrder_ShouldCarryErrorTypeAndServiceId()
    {
        var serviceId = Guid.NewGuid();

        var exception = new ServiceAlreadyAddedToServiceOrderException(serviceId);

        exception.ErrorType.ShouldBe("service-already-added-to-service-order");
        exception.Message.ShouldContain(serviceId.ToString());
    }
}
