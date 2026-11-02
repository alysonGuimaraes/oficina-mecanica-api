using AutoGarage.Domain.Entities;

namespace AutoGarage.Application.Services.Notifications;

public interface IServiceOrderStatusNotifier
{
    // Never throws: a failed notification must not change or fail the service order operation.
    Task NotifyAsync(ServiceOrder order, ServiceOrderStatusHistory history, CancellationToken cancellationToken);
}
