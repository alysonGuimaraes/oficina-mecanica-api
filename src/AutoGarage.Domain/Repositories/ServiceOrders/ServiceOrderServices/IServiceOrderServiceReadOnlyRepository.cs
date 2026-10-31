using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;

public interface IServiceOrderServiceReadOnlyRepository
{
    Task<ServiceOrderService?> GetById(Guid id, CancellationToken cancellationToken);

    Task<ServiceOrderService?> GetByServiceOrderAndServiceId(Guid serviceOrderId, Guid serviceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceOrderService>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken);
}
