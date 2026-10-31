using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;

namespace AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;

public class FakeServiceOrderRepository : IServiceOrderReadOnlyRepository, IServiceOrderWriteOnlyRepository
{
    private readonly List<ServiceOrder> _orders = [];

    public IReadOnlyList<ServiceOrder> Orders => _orders;

    public Task<ServiceOrder?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    }

    public Task<IReadOnlyList<ServiceOrder>> GetAll(ServiceOrderStatus? status, CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceOrder> result = _orders
            .Where(o => status == null || o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .ToList();

        return Task.FromResult(result);
    }

    public Task Add(ServiceOrder serviceOrder, CancellationToken cancellationToken)
    {
        _orders.Add(serviceOrder);

        return Task.CompletedTask;
    }

    public Task Update(ServiceOrder serviceOrder, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
