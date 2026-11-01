namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderItemNotActiveException : ConflictDomainException
{
    public ServiceOrderItemNotActiveException(Guid id)
        : base($"Service order item '{id}' is not active.", "service-order-item-not-active")
    {
    }
}
