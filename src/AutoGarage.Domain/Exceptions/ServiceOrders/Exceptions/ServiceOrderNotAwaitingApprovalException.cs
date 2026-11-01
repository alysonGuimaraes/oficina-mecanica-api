using AutoGarage.Domain.Common.Exceptions;

namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

public class ServiceOrderNotAwaitingApprovalException : ConflictDomainException
{
    public ServiceOrderNotAwaitingApprovalException()
        : base("The service order is not awaiting approval.", "service-order-not-awaiting-approval")
    {
    }
}
