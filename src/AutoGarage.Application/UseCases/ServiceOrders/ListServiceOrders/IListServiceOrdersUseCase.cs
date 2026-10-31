using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.ListServiceOrders;

public interface IListServiceOrdersUseCase
{
    Task<IReadOnlyList<ServiceOrderResponse>> Handle(string? status, CancellationToken cancellationToken);
}
