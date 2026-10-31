namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidServiceOrderItemIdException : BadRequestDomainException
{
    public InvalidServiceOrderItemIdException()
        : base("The provided item id is invalid.", "invalid-service-order-item-id")
    {
    }
}
