using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.AddMaterialToServiceOrder;

public interface IAddMaterialToServiceOrderUseCase
{
    Task<ServiceOrderMaterialItemResponse> Handle(AddMaterialToServiceOrderCommand command, CancellationToken cancellationToken);
}
