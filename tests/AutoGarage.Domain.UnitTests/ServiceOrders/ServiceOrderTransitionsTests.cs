using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.StateMachines;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderTransitionsTests
{
    private static readonly (ServiceOrderStatus From, ServiceOrderStatus To)[] AllowedTransitions =
    [
        (ServiceOrderStatus.RECEIVED, ServiceOrderStatus.IN_DIAGNOSIS),
        (ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.AWAITING_APPROVAL),
        (ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.FINISHED),
        (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.IN_EXECUTION),
        (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.AWAITING_MATERIALS),
        (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.FINISHED),
        (ServiceOrderStatus.AWAITING_MATERIALS, ServiceOrderStatus.IN_EXECUTION),
        (ServiceOrderStatus.IN_EXECUTION, ServiceOrderStatus.FINISHED),
        (ServiceOrderStatus.FINISHED, ServiceOrderStatus.DELIVERED),
        (ServiceOrderStatus.CANCELLED, ServiceOrderStatus.DELIVERED),
        (ServiceOrderStatus.RECEIVED, ServiceOrderStatus.CANCELLED),
        (ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.CANCELLED),
        (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.CANCELLED),
        (ServiceOrderStatus.AWAITING_MATERIALS, ServiceOrderStatus.CANCELLED),
        (ServiceOrderStatus.IN_EXECUTION, ServiceOrderStatus.CANCELLED)
    ];

    [Fact]
    public void IsAllowed_ShouldMatchSpecMatrix()
    {
        foreach (var from in Enum.GetValues<ServiceOrderStatus>())
        {
            foreach (var to in Enum.GetValues<ServiceOrderStatus>())
            {
                var expected = AllowedTransitions.Contains((from, to));

                ServiceOrderTransitions.IsAllowed(from, to).ShouldBe(expected, $"{from} -> {to}");
            }
        }
    }

    [Fact]
    public void InvalidServiceOrderTransitionException_ShouldCarryErrorTypeAndStatuses()
    {
        var exception = new InvalidServiceOrderTransitionException(ServiceOrderStatus.DELIVERED, ServiceOrderStatus.RECEIVED);

        exception.ErrorType.ShouldBe("invalid-service-order-transition");
        exception.Message.ShouldContain("DELIVERED");
        exception.Message.ShouldContain("RECEIVED");
    }
}
