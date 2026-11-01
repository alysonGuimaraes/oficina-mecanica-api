using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;

namespace AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;

public class FakeServiceOrderServiceRepository : IServiceOrderServiceReadOnlyRepository, IServiceOrderServiceWriteOnlyRepository
{
    private readonly List<ServiceOrderService> _items = [];

    public IReadOnlyList<ServiceOrderService> Items => _items;

    public Task<ServiceOrderService?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
    }

    public Task<ServiceOrderService?> GetByServiceOrderAndServiceId(Guid serviceOrderId, Guid serviceId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(i => i.ServiceOrderId == serviceOrderId && i.ServiceId == serviceId));
    }

    public Task<IReadOnlyList<ServiceOrderService>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceOrderService> result = _items
            .Where(i => i.ServiceOrderId == serviceOrderId)
            .ToList();

        return Task.FromResult(result);
    }

    public Task Add(ServiceOrderService item, CancellationToken cancellationToken)
    {
        _items.Add(item);

        return Task.CompletedTask;
    }

    public Task Update(ServiceOrderService item, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
