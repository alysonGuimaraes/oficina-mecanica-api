using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;

public interface IServiceOrderStatusHistoryWriteOnlyRepository
{
    Task Add(ServiceOrderStatusHistory history, CancellationToken cancellationToken);
}
