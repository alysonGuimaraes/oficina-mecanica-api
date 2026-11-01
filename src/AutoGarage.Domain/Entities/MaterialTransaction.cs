using AutoGarage.Domain.Enums.MaterialTransactions;

namespace AutoGarage.Domain.Entities;

public class MaterialTransaction
{
    public Guid Id { get; private set; }
    public Guid MaterialId { get; private set; }
    public TransactionType Type { get; private set; }
    public int Quantity { get; private set; }
    public int BalanceAfter { get; private set; }
    public Guid? ServiceOrderId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected MaterialTransaction() { }

    private MaterialTransaction(Guid materialId, TransactionType type, int quantity, int balanceAfter, Guid? serviceOrderId)
    {
        Id = Guid.NewGuid();
        MaterialId = materialId;
        Type = type;
        Quantity = quantity;
        BalanceAfter = balanceAfter;
        ServiceOrderId = serviceOrderId;
        CreatedAt = DateTime.UtcNow;
    }

    public static MaterialTransaction Create(Guid materialId, TransactionType type, int quantity, int balanceAfter, Guid? serviceOrderId = null)
    {
        return new MaterialTransaction(materialId, type, quantity, balanceAfter, serviceOrderId);
    }
}
