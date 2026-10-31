namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderCustomerInactiveException : ConflictDomainException
{
    public ServiceOrderCustomerInactiveException(Guid customerId)
        : base($"Customer with id '{customerId}' is inactive and cannot open service orders.", "service-order-customer-inactive")
    {
    }
}
