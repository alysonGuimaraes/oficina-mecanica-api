using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Employees;

public class EmployeeTests
{

    #region Success Tests
    [Fact]
    public void Create_WithValidData_ShouldCreateEmployeeAsActive()
    {
        // Arrange
        var name = "John Doe";
        var email = "john@autogarage.com";
        var passwordHash = "$2a$11$ValidHashStringMocked";
        var role = EmployeeRole.MECHANIC;

        // Act
        var employee = Employee.Create(name, email, passwordHash, role);

        // Assert
        employee.ShouldNotBeNull();
        employee.Id.ShouldNotBe(Guid.Empty);
        employee.Name.ShouldBe(name);
        employee.Email.ShouldBe(email);
        employee.PasswordHash.ShouldBe(passwordHash);
        employee.Role.ShouldBe(role);
        employee.Status.ShouldBe(EntityStatus.ACTIVE);
        employee.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateEmployeeData()
    {
        // Arrange
        var employee = Employee.Create("Tony Stark", "TonyStark@autogarage.com", "hash", EmployeeRole.MECHANIC);
        var newName = "Tony Stank";
        var newEmail = "TonyStank@autogarage.com";
        var newRole = EmployeeRole.ATTENDANT;

        // Act
        employee.UpdateProfile(newName, newEmail, newRole);

        // Assert
        employee.ShouldNotBeNull();
        employee.Id.ShouldNotBe(Guid.Empty);
        employee.Name.ShouldBe(newName);
        employee.Email.ShouldBe(newEmail);
        employee.Role.ShouldBe(newRole);
        employee.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void Activate_InactiveEmployee_ShouldChangeStatusToActive()
    {
        // Arrange
        var inactiveEmployee = Employee.Create("Tony Stark", "TonyStark@autogarage.com", "hash", EmployeeRole.MECHANIC);
        inactiveEmployee.Deactivate();

        // Act
        inactiveEmployee.Activate();

        // Assert
        inactiveEmployee.Status.ShouldBe(EntityStatus.ACTIVE);
        inactiveEmployee.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Deactivate_ActiveEmployee_ShouldChangeStatusToInactive()
    {
        // Arrange
        var employee = Employee.Create("John Doe", "john@autogarage.com", "hash", EmployeeRole.ATTENDANT);

        // Act
        employee.Deactivate();

        // Assert
        employee.Status.ShouldBe(EntityStatus.INACTIVE);
        employee.UpdatedAt.ShouldNotBeNull();
    }

    #endregion

    #region Fail Tests
    [Fact]
    public void Create_WithInvalidRole_ShouldThrowInvalidEmployeeRoleException()
    {
        // Arrange
        var invalidRole = (EmployeeRole)99;

        // Act & Assert
        Should.Throw<InvalidEmployeeRoleException>(() =>
        {
            Employee.Create("Jane Doe", "jane@autogarage.com", "hash", invalidRole);
        });
    }

    [Fact]
    public void Update_WithInvalidRole_ShouldThrowInvalidEmployeeRoleException()
    {
        // Arrange
        var employee = Employee.Create("Jane Doe", "jane@autogarage.com", "hash", EmployeeRole.MECHANIC);
        var invalidRole = (EmployeeRole)99;

        // Act & Assert
        Should.Throw<InvalidEmployeeRoleException>(() =>
        {
            employee.UpdateProfile("Jane Doe", "jane@autogarage.com", invalidRole);
        });
    }

    [Fact]
    public void Activate_AlreadyActiveEmployee_ShouldThrowEmployeeAlreadyActiveException()
    {
        // Arrange
        var employee = Employee.Create("Jane Doe", "jane@autogarage.com", "hash", EmployeeRole.MECHANIC);

        // Act & Assert
        Should.Throw<EmployeeAlreadyActiveException>(() =>
        {
            employee.Activate();
        });
    }

    [Fact]
    public void Deactivate_AlreadyInactiveEmployee_ShouldThrowEmployeeAlreadyInactiveException()
    {
        // Arrange
        var employee = Employee.Create("Jane Doe", "jane@autogarage.com", "hash", EmployeeRole.MECHANIC);
        employee.Deactivate();

        // Act & Assert
        Should.Throw<EmployeeAlreadyInactiveException>(() =>
        {
            employee.Deactivate();
        });
    }
    #endregion

}
