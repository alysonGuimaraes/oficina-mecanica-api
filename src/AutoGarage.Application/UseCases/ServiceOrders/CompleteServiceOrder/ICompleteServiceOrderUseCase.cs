using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.CompleteServiceOrder;

public interface ICompleteServiceOrderUseCase
{
    Task<ServiceOrderResponse> Handle(CompleteServiceOrderCommand command, CancellationToken cancellationToken);
}
