using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;

namespace AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;

public class FakeServiceOrderStatusHistoryRepository : IServiceOrderStatusHistoryWriteOnlyRepository
{
    private readonly List<ServiceOrderStatusHistory> _histories = [];

    public IReadOnlyList<ServiceOrderStatusHistory> Histories => _histories;

    public Task Add(ServiceOrderStatusHistory history, CancellationToken cancellationToken)
    {
        _histories.Add(history);

        return Task.CompletedTask;
    }
}
