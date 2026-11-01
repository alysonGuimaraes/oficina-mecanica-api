using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Utilities;

public static class ServiceOrderBudgetCalculator
{
    public static decimal Calculate(
        IReadOnlyList<ServiceOrderService> services,
        IReadOnlyList<ServiceOrderMaterial> materials,
        IReadOnlySet<Guid> inactiveCatalogServiceIds)
    {
        var activeServices = services.Where(s => s.Status == EntityStatus.ACTIVE).ToList();

        var blockedService = activeServices.FirstOrDefault(s => inactiveCatalogServiceIds.Contains(s.ServiceId));

        if (blockedService is not null)
        {
            throw new ServiceOrderBudgetInactiveServiceException(blockedService.ServiceId);
        }

        if (activeServices.Count == 0)
        {
            throw new ServiceOrderBudgetWithoutItemsException();
        }

        var activeMaterials = materials.Where(m => m.Status == EntityStatus.ACTIVE).ToList();

        var servicesTotal = activeServices.Sum(s => s.Price);
        var materialsTotal = activeMaterials.Sum(m => m.UnitPrice * m.Quantity);

        return servicesTotal + materialsTotal;
    }
}
