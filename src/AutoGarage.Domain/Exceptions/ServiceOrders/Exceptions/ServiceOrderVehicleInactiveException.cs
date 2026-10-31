namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderVehicleInactiveException : ConflictDomainException
{
    public ServiceOrderVehicleInactiveException(Guid vehicleId)
        : base($"Vehicle with id '{vehicleId}' is inactive and cannot be used in service orders.", "service-order-vehicle-inactive")
    {
    }
}
