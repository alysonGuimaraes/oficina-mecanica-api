namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderServiceInactiveException : ConflictDomainException
{
    public ServiceOrderServiceInactiveException(Guid serviceId)
        : base($"Service '{serviceId}' is inactive and cannot be added to a service order.", "service-order-service-inactive")
    {
    }
}
