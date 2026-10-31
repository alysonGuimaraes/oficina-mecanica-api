using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Utilities;

public static class ServiceOrderBudgetCalculator
{
    public static decimal Calculate(IReadOnlyList<ServiceOrderService> services, IReadOnlyList<ServiceOrderMaterial> materials)
    {
        var inactiveService = services.FirstOrDefault(s => s.Status == EntityStatus.INACTIVE);

        if (inactiveService is not null)
        {
            throw new ServiceOrderBudgetInactiveServiceException(inactiveService.ServiceId);
        }

        var activeServices = services.Where(s => s.Status == EntityStatus.ACTIVE).ToList();
        var activeMaterials = materials.Where(m => m.Status == EntityStatus.ACTIVE).ToList();

        if (activeServices.Count == 0 && activeMaterials.Count == 0)
        {
            throw new ServiceOrderBudgetWithoutItemsException();
        }

        var servicesTotal = activeServices.Sum(s => s.Price);
        var materialsTotal = activeMaterials.Sum(m => m.UnitPrice * m.Quantity);

        return servicesTotal + materialsTotal;
    }
}
