using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Entities;

public class ServiceOrderMaterial
{
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid MaterialId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected ServiceOrderMaterial() { }

    private ServiceOrderMaterial(Guid serviceOrderId, Guid materialId, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ServiceOrderId = serviceOrderId;
        MaterialId = materialId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Status = EntityStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
    }

    public static ServiceOrderMaterial Create(Guid serviceOrderId, Guid materialId, int quantity, decimal unitPrice)
    {
        return new ServiceOrderMaterial(serviceOrderId, materialId, quantity, unitPrice);
    }

    // Material repetido com a linha ativa soma a quantidade.
    public void AddQuantity(int quantity)
    {
        if (Status == EntityStatus.INACTIVE)
        {
            throw new ServiceOrderItemNotActiveException(Id);
        }

        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;
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

    // Reativa a mesma linha. A quantidade informada substitui a anterior, e não soma.
    public void Reactivate(int quantity, decimal unitPrice)
    {
        if (Status == EntityStatus.ACTIVE)
        {
            throw new ServiceOrderItemAlreadyActiveException(Id);
        }

        Status = EntityStatus.ACTIVE;
        Quantity = quantity;
        UnitPrice = unitPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
