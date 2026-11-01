using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.ListPublicServiceOrders;

public interface IListPublicServiceOrdersUseCase
{
    Task<IReadOnlyList<ServiceOrderResponse>> Handle(string? document, CancellationToken cancellationToken);
}
