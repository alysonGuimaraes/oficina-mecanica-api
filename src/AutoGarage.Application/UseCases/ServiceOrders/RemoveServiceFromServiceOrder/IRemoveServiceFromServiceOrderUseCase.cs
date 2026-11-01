using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.RemoveServiceFromServiceOrder;

public interface IRemoveServiceFromServiceOrderUseCase
{
    Task<ServiceOrderServiceItemResponse> Handle(RemoveServiceFromServiceOrderCommand command, CancellationToken cancellationToken);
}
