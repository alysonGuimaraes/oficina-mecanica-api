using AutoGarage.Domain.Common.Exceptions;

namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

public class ServiceOrderApprovalExpiredException : BadRequestDomainException
{
    public ServiceOrderApprovalExpiredException()
        : base("The approval deadline has expired.", "service-order-approval-expired")
    {
    }
}
