using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.CreateServiceOrder;

public interface ICreateServiceOrderUseCase
{
    Task<ServiceOrderResponse> Handle(CreateServiceOrderCommand command, CancellationToken cancellationToken);
}
