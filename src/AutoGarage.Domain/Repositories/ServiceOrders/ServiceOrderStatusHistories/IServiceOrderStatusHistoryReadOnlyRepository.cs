using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;

public interface IServiceOrderStatusHistoryReadOnlyRepository
{
    Task<IReadOnlyList<ServiceOrderStatusHistory>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken);
}
