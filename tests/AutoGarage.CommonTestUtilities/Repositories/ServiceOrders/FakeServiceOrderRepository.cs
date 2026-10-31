using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;

namespace AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;

public class FakeServiceOrderRepository : IServiceOrderWriteOnlyRepository
{
    private readonly List<ServiceOrder> _orders = [];

    public IReadOnlyList<ServiceOrder> Orders => _orders;

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
