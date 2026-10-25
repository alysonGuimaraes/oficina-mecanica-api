using AutoGarage.Domain.Employees.Enums;
using AutoGarage.Domain.Employees.Exceptions;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Employees;

public class Employee
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public EmployeeRole Role { get; private set; }
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Employee() { }

    private Employee(string name, string email, string passwordHash, EmployeeRole role)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        Status = EntityStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Employee Create(string name, string email, string passwordHash, EmployeeRole role)
    {
        if (!Enum.IsDefined(typeof(EmployeeRole), role))
        {
            throw new InvalidEmployeeRoleException(role.ToString());
        }

        return new Employee(name, email, passwordHash, role);
    }

    public void UpdateProfile(string name, string email, EmployeeRole role)
    {
        if (!Enum.IsDefined(typeof(EmployeeRole), role))
        {
            throw new InvalidEmployeeRoleException(role.ToString());
        }

        Name = name;
        Email = email;
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == EntityStatus.Active)
        {
            throw new EmployeeAlreadyActiveException(Id);
        }

        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.Inactive)
        {
            throw new EmployeeAlreadyInactiveException(Id);
        }

        Status = EntityStatus.Inactive; 
        UpdatedAt = DateTime.UtcNow;
    }
}
