using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.PublicServiceOrderApproval;

public interface IPublicServiceOrderApprovalUseCase
{
    Task<ServiceOrderApprovalResponse> Handle(PublicServiceOrderApprovalCommand command, CancellationToken cancellationToken);
}
