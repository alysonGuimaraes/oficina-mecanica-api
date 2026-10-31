using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;

public interface IServiceOrderReadOnlyRepository
{
    Task<ServiceOrder?> GetById(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceOrder>> GetAll(ServiceOrderStatus? status, CancellationToken cancellationToken);
}
