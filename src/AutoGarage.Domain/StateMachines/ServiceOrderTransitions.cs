using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Domain.StateMachines;

public static class ServiceOrderTransitions
{
    public static bool IsAllowed(ServiceOrderStatus from, ServiceOrderStatus to)
    {
        return (from, to) switch
        {
            (ServiceOrderStatus.RECEIVED, ServiceOrderStatus.IN_DIAGNOSIS) => true,
            (ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.AWAITING_APPROVAL) => true,
            (ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.FINISHED) => true,
            (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.IN_EXECUTION) => true,
            (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.AWAITING_MATERIALS) => true,
            (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.FINISHED) => true,
            (ServiceOrderStatus.AWAITING_MATERIALS, ServiceOrderStatus.IN_EXECUTION) => true,
            (ServiceOrderStatus.IN_EXECUTION, ServiceOrderStatus.FINISHED) => true,
            (ServiceOrderStatus.FINISHED, ServiceOrderStatus.DELIVERED) => true,
            (ServiceOrderStatus.CANCELLED, ServiceOrderStatus.DELIVERED) => true,
            (ServiceOrderStatus.RECEIVED, ServiceOrderStatus.CANCELLED) => true,
            (ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.CANCELLED) => true,
            (ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.CANCELLED) => true,
            (ServiceOrderStatus.AWAITING_MATERIALS, ServiceOrderStatus.CANCELLED) => true,
            (ServiceOrderStatus.IN_EXECUTION, ServiceOrderStatus.CANCELLED) => true,
            _ => false
        };
    }
}
