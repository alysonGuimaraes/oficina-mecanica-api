namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceAlreadyAddedToServiceOrderException : BadRequestDomainException
{
    public ServiceAlreadyAddedToServiceOrderException(Guid serviceId)
        : base($"Service '{serviceId}' is already active in this service order.", "service-already-added-to-service-order")
    {
    }
}
