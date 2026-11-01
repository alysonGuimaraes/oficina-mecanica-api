using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;

namespace AutoGarage.CommonTestUtilities.Repositories.ServiceOrders;

public class FakeServiceOrderMaterialRepository : IServiceOrderMaterialReadOnlyRepository, IServiceOrderMaterialWriteOnlyRepository
{
    private readonly List<ServiceOrderMaterial> _items = [];

    public IReadOnlyList<ServiceOrderMaterial> Items => _items;

    public Task<ServiceOrderMaterial?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
    }

    public Task<ServiceOrderMaterial?> GetByServiceOrderAndMaterialId(Guid serviceOrderId, Guid materialId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.FirstOrDefault(i => i.ServiceOrderId == serviceOrderId && i.MaterialId == materialId));
    }

    public Task<IReadOnlyList<ServiceOrderMaterial>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceOrderMaterial> result = _items
            .Where(i => i.ServiceOrderId == serviceOrderId)
            .ToList();

        return Task.FromResult(result);
    }

    public Task Add(ServiceOrderMaterial item, CancellationToken cancellationToken)
    {
        _items.Add(item);

        return Task.CompletedTask;
    }

    public Task Update(ServiceOrderMaterial item, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
