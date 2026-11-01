namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidServiceOrderStatusException : BadRequestDomainException
{
    public InvalidServiceOrderStatusException()
        : base("The provided status is invalid. Allowed values are RECEIVED, IN_DIAGNOSIS, AWAITING_APPROVAL, AWAITING_MATERIALS, IN_EXECUTION, FINISHED, CANCELLED or DELIVERED.", "invalid-service-order-status")
    {
    }
}
