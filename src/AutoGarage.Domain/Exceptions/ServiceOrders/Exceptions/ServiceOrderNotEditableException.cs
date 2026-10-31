namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderNotEditableException : ConflictDomainException
{
    public ServiceOrderNotEditableException()
        : base("Items can only be changed while the service order is IN_DIAGNOSIS.", "service-order-not-editable")
    {
    }
}
