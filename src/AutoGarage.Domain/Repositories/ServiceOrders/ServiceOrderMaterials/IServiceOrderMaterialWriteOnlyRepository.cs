using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;

public interface IServiceOrderMaterialWriteOnlyRepository
{
    Task Add(ServiceOrderMaterial item, CancellationToken cancellationToken);

    Task Update(ServiceOrderMaterial item, CancellationToken cancellationToken);
}
