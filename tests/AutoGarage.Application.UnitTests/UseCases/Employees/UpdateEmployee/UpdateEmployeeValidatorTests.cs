using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.UseCases.Employees.UpdateEmployee;
using AutoGarage.Domain.Enums.Employees;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Employees.UpdateEmployee;

public class UpdateEmployeeValidatorTests
{
    private const string ValidId = "a4443373-c931-42b0-b099-ab379f3264a4";

    [Theory]
    [InlineData("", "john@autogarage.com", "The name field is required.")]
    [InlineData("John", "", "The email field is required.")]
    [InlineData("John", "invalid-email", "The provided email format is invalid.")]
    public void Validate_InvalidField_ShouldReturnExpectedMessage(string name, string email, string expectedMessage)
    {
        var command = new UpdateEmployeeCommand(ValidId, name, email, EmployeeRole.MECHANIC);

        var result = new UpdateEmployeeValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage).ShouldContain(expectedMessage);
    }

    [Fact]
    public void Validate_InvalidRole_ShouldReturnRoleMessage()
    {
        var command = new UpdateEmployeeCommand(ValidId, "John", "john@autogarage.com", (EmployeeRole)99);

        var result = new UpdateEmployeeValidator().Validate(command);

        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("The provided role is invalid. Allowed roles are MECHANIC or ATTENDANT.");
    }

    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        var command = new UpdateEmployeeCommand(ValidId, "John", "john@autogarage.com", EmployeeRole.ATTENDANT);

        var result = new UpdateEmployeeValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
