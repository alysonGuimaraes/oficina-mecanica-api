namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderVehicleCustomerMismatchException : ConflictDomainException
{
    public ServiceOrderVehicleCustomerMismatchException(Guid vehicleId, Guid customerId)
        : base($"Vehicle with id '{vehicleId}' does not belong to customer '{customerId}'.", "service-order-vehicle-customer-mismatch")
    {
    }
}
