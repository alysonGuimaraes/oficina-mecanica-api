namespace AutoGarage.UnitTests.Domain.Employees;

using AutoGarage.Domain.Employees;
using AutoGarage.Domain.Employees.Enums;
using AutoGarage.Domain.Employees.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

public class EmployeeTests
{

    #region Success Tests
    [Fact]
	public void create_WithValidData_ShouldCreateEmployeeAsActive()
	{
        // Arrange
        var name = "John Doe";
        var email = "john@autogarage.com";
        var passwordHash = "$2a$11$ValidHashStringMocked";
        var role = EmployeeRole.Mechanic;

        // Act
        var employee = Employee.Create(name, email, passwordHash, role);

        // Assert
        employee.ShouldNotBeNull();
        employee.Id.ShouldNotBe(Guid.Empty);
        employee.Name.ShouldBe(name);
        employee.Email.ShouldBe(email);
        employee.PasswordHash.ShouldBe(passwordHash);
        employee.Role.ShouldBe(role);
        employee.Status.ShouldBe(EntityStatus.Active);
        employee.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateEmployeeData()
    {
        // Arrange
        var employee = Employee.Create("Tony Stark", "TonyStark@autogarage.com", "hash", EmployeeRole.Mechanic);
        var newName = "Tony Stank";
        var newEmail = "TonyStank@autogarage.com";
        var newRole = EmployeeRole.Attendant;

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
        var inactiveEmployee = Employee.Create("Tony Stark", "TonyStark@autogarage.com", "hash", EmployeeRole.Mechanic);
        inactiveEmployee.Deactivate();

        // Act
        inactiveEmployee.Activate();

        // Assert
        inactiveEmployee.Status.ShouldBe(EntityStatus.Active);
        inactiveEmployee.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Deactivate_ActiveEmployee_ShouldChangeStatusToInactive()
    {
        // Arrange
        var employee = Employee.Create("John Doe", "john@autogarage.com", "hash", EmployeeRole.Attendant);

        // Act
        employee.Deactivate();

        // Assert
        employee.Status.ShouldBe(EntityStatus.Inactive);
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
        var employee = Employee.Create("Jane Doe", "jane@autogarage.com", "hash", EmployeeRole.Mechanic);
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
        var employee = Employee.Create("Jane Doe", "jane@autogarage.com", "hash", EmployeeRole.Mechanic);

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
        var employee = Employee.Create("Jane Doe", "jane@autogarage.com", "hash", EmployeeRole.Mechanic);
        employee.Deactivate();

        // Act & Assert
        Should.Throw<EmployeeAlreadyInactiveException>(() =>
        {
            employee.Deactivate();
        });
    }
    #endregion

}
