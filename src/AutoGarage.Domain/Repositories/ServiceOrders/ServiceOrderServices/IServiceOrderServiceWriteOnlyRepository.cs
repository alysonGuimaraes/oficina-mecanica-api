using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;

public interface IServiceOrderServiceWriteOnlyRepository
{
    Task Add(ServiceOrderService item, CancellationToken cancellationToken);

    Task Update(ServiceOrderService item, CancellationToken cancellationToken);
}
