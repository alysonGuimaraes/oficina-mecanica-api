using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidServiceOrderTransitionException : ConflictDomainException
{
    public InvalidServiceOrderTransitionException(ServiceOrderStatus from, ServiceOrderStatus to)
        : base($"Service order cannot move from {from} to {to}.", "invalid-service-order-transition")
    {
    }
}
