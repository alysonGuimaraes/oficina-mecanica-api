using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.DeliverServiceOrder;

public interface IDeliverServiceOrderUseCase
{
    Task<ServiceOrderResponse> Handle(ServiceOrderEmployeeCommand command, CancellationToken cancellationToken);
}
