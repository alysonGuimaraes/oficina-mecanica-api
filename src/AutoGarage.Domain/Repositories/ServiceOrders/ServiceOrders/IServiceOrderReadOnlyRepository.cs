using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;

public interface IServiceOrderReadOnlyRepository
{
    Task<ServiceOrder?> GetById(Guid id, CancellationToken cancellationToken);
}
