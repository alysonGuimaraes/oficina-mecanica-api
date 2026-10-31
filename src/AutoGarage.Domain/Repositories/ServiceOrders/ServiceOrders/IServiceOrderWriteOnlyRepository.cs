using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;

public interface IServiceOrderWriteOnlyRepository
{
    Task Add(ServiceOrder serviceOrder, CancellationToken cancellationToken);

    Task Update(ServiceOrder serviceOrder, CancellationToken cancellationToken);
}
