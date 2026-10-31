using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.RemoveMaterialFromServiceOrder;

public interface IRemoveMaterialFromServiceOrderUseCase
{
    Task<ServiceOrderMaterialItemResponse> Handle(RemoveMaterialFromServiceOrderCommand command, CancellationToken cancellationToken);
}
