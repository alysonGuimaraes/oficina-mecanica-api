using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderBudgetExceptionsTests
{
    [Fact]
    public void BudgetWithoutItems_ShouldCarryErrorType()
    {
        var exception = new ServiceOrderBudgetWithoutItemsException();

        exception.ErrorType.ShouldBe("service-order-budget-without-items");
    }

    [Fact]
    public void BudgetInactiveService_ShouldCarryErrorTypeAndServiceId()
    {
        var serviceId = Guid.NewGuid();

        var exception = new ServiceOrderBudgetInactiveServiceException(serviceId);

        exception.ErrorType.ShouldBe("service-order-budget-inactive-service");
        exception.Message.ShouldContain(serviceId.ToString());
    }
}
