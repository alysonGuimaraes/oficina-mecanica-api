using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.AddServiceToServiceOrder;

public interface IAddServiceToServiceOrderUseCase
{
    Task<ServiceOrderServiceItemResponse> Handle(AddServiceToServiceOrderCommand command, CancellationToken cancellationToken);
}
