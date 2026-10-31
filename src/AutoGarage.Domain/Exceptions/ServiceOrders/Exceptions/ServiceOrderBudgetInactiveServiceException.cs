namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderBudgetInactiveServiceException : ConflictDomainException
{
    public ServiceOrderBudgetInactiveServiceException(Guid serviceId)
        : base($"Service '{serviceId}' is inactive and blocks the budget of this service order.", "service-order-budget-inactive-service")
    {
    }
}
