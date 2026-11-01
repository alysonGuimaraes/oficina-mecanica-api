namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderItemNotFoundException : NotFoundDomainException
{
    public ServiceOrderItemNotFoundException(Guid id)
        : base($"Service order item with id '{id}' was not found.", "service-order-item-not-found")
    {
    }
}
