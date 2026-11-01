namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderMaterialInactiveException : ConflictDomainException
{
    public ServiceOrderMaterialInactiveException(Guid materialId)
        : base($"Material '{materialId}' is inactive and cannot be added to a service order.", "service-order-material-inactive")
    {
    }
}
