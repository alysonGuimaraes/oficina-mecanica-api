using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.CancelServiceOrder;

public interface ICancelServiceOrderUseCase
{
    Task<ServiceOrderResponse> Handle(CancelServiceOrderCommand command, CancellationToken cancellationToken);
}
