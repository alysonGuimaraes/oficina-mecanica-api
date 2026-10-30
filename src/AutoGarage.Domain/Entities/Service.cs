using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Entities;

public class Service
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Service() { }

    private Service(string name, string? description, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Status = EntityStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
    }

    public static Service Create(string name, string? description, decimal price)
    {
        return new Service(name, description, price);
    }

    public void Activate()
    {
        if (Status == EntityStatus.ACTIVE)
        {
            throw new ServiceAlreadyActiveException(Id);
        }

        Status = EntityStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.INACTIVE)
        {
            throw new ServiceAlreadyInactiveException(Id);
        }

        Status = EntityStatus.INACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }
}
