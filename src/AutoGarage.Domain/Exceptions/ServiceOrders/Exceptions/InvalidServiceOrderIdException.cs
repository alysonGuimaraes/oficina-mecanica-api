namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidServiceOrderIdException : BadRequestDomainException
{
    public InvalidServiceOrderIdException()
        : base("The provided id is invalid.", "invalid-service-order-id")
    {
    }
}
