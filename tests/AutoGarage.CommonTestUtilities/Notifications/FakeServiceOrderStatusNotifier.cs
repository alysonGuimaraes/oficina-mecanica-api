using AutoGarage.Application.Services.Notifications;
using AutoGarage.Domain.Entities;

namespace AutoGarage.CommonTestUtilities.Notifications;

// Records every status change handed to the notifier, without sending anything.
public class FakeServiceOrderStatusNotifier : IServiceOrderStatusNotifier
{
    private readonly List<(ServiceOrder Order, ServiceOrderStatusHistory History)> _notified = [];

    public IReadOnlyList<(ServiceOrder Order, ServiceOrderStatusHistory History)> Notified => _notified;

    public Task NotifyAsync(ServiceOrder order, ServiceOrderStatusHistory history, CancellationToken cancellationToken)
    {
        _notified.Add((order, history));

        return Task.CompletedTask;
    }
}
