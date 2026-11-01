namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderBudgetWithoutItemsException : ConflictDomainException
{
    public ServiceOrderBudgetWithoutItemsException()
        : base("The service order has no active item to send as budget.", "service-order-budget-without-items")
    {
    }
}
