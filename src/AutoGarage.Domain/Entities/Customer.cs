using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;

namespace AutoGarage.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Document Document { get; private set; } = null!;
    public DocumentType DocumentType { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Ddd { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Customer() { }

    private Customer(string name, Document document, string email, string ddd, string phoneNumber)
    {
        Id = Guid.NewGuid();
        Name = name;
        Document = document;
        DocumentType = document.Type;
        Email = email;
        Ddd = ddd;
        PhoneNumber = phoneNumber;
        Status = EntityStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
    }

    public static Customer Create(string name, Document document, string email, string ddd, string phoneNumber)
    {
        return new Customer(name, document, email, ddd, phoneNumber);
    }

    public void Activate()
    {
        if (Status == EntityStatus.ACTIVE)
        {
            throw new CustomerAlreadyActiveException(Id);
        }

        Status = EntityStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.INACTIVE)
        {
            throw new CustomerAlreadyInactiveException(Id);
        }

        Status = EntityStatus.INACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }
}
