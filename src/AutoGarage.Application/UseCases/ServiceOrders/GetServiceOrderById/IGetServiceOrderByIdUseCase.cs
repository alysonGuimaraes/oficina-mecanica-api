using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.GetServiceOrderById;

public interface IGetServiceOrderByIdUseCase
{
    Task<ServiceOrderDetailResponse> Handle(string id, CancellationToken cancellationToken);
}
