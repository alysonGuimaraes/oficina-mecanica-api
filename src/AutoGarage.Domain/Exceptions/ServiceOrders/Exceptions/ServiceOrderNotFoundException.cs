namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderNotFoundException : NotFoundDomainException
{
    public ServiceOrderNotFoundException(Guid id)
        : base($"Service order with id '{id}' was not found.", "service-order-not-found")
    {
    }
}
