using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Entities;

public class Material
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int StockMinimum { get; private set; }
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Material() { }

    private Material(string name, string? description, decimal price, int stockMinimum)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = 0;
        StockMinimum = stockMinimum;
        Status = EntityStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
    }

    public static Material Create(string name, string? description, decimal price, int stockMinimum)
    {
        return new Material(name, description, price, stockMinimum);
    }

    public void Activate()
    {
        if (Status == EntityStatus.ACTIVE)
        {
            throw new MaterialAlreadyActiveException(Id);
        }

        Status = EntityStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.INACTIVE)
        {
            throw new MaterialAlreadyInactiveException(Id);
        }

        Status = EntityStatus.INACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }
}
