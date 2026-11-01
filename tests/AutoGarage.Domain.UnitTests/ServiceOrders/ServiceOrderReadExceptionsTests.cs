using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderReadExceptionsTests
{
    [Fact]
    public void InvalidServiceOrderStatus_ShouldCarryErrorType()
    {
        var exception = new InvalidServiceOrderStatusException();

        exception.ErrorType.ShouldBe("invalid-service-order-status");
    }

    [Fact]
    public void InvalidServiceOrderId_ShouldCarryErrorType()
    {
        var exception = new InvalidServiceOrderIdException();

        exception.ErrorType.ShouldBe("invalid-service-order-id");
    }

    [Fact]
    public void ServiceOrderNotFound_ShouldCarryErrorTypeAndId()
    {
        var id = Guid.NewGuid();

        var exception = new ServiceOrderNotFoundException(id);

        exception.ErrorType.ShouldBe("service-order-not-found");
        exception.Message.ShouldContain(id.ToString());
    }
}
