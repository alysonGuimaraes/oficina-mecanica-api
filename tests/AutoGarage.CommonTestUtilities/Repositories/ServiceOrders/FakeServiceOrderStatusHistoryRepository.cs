using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;

namespace AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;

public class FakeServiceOrderStatusHistoryRepository : IServiceOrderStatusHistoryWriteOnlyRepository, IServiceOrderStatusHistoryReadOnlyRepository
{
    private readonly List<ServiceOrderStatusHistory> _histories = [];

    public IReadOnlyList<ServiceOrderStatusHistory> Histories => _histories;

    public Task<IReadOnlyList<ServiceOrderStatusHistory>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceOrderStatusHistory> result = _histories
            .Where(h => h.ServiceOrderId == serviceOrderId)
            .ToList();

        return Task.FromResult(result);
    }

    public Task Add(ServiceOrderStatusHistory history, CancellationToken cancellationToken)
    {
        _histories.Add(history);

        return Task.CompletedTask;
    }
}
