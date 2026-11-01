using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Entities;

public class ServiceOrderService
{
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public decimal Price { get; private set; }
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected ServiceOrderService() { }

    private ServiceOrderService(Guid serviceOrderId, Guid serviceId, decimal price)
    {
        Id = Guid.NewGuid();
        ServiceOrderId = serviceOrderId;
        ServiceId = serviceId;
        Price = price;
        Status = EntityStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
    }

    public static ServiceOrderService Create(Guid serviceOrderId, Guid serviceId, decimal price)
    {
        return new ServiceOrderService(serviceOrderId, serviceId, price);
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.INACTIVE)
        {
            throw new ServiceOrderItemNotActiveException(Id);
        }

        Status = EntityStatus.INACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }

    // Reativa a mesma linha, com o preço atual do catálogo.
    public void Reactivate(decimal price)
    {
        if (Status == EntityStatus.ACTIVE)
        {
            throw new ServiceOrderItemAlreadyActiveException(Id);
        }

        Status = EntityStatus.ACTIVE;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }
}
