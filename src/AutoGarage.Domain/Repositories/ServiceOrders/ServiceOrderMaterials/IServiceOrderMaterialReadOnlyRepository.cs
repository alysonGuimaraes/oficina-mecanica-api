using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;

public interface IServiceOrderMaterialReadOnlyRepository
{
    Task<ServiceOrderMaterial?> GetById(Guid id, CancellationToken cancellationToken);

    Task<ServiceOrderMaterial?> GetByServiceOrderAndMaterialId(Guid serviceOrderId, Guid materialId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceOrderMaterial>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken);
}
