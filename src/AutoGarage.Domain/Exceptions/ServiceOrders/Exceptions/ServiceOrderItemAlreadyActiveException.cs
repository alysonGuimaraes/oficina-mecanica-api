namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderItemAlreadyActiveException : ConflictDomainException
{
    public ServiceOrderItemAlreadyActiveException(Guid id)
        : base($"Service order item '{id}' is already active.", "service-order-item-already-active")
    {
    }
}
