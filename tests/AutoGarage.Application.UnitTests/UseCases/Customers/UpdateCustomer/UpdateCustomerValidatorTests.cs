using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.UseCases.Customers.UpdateCustomer;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Customers.UpdateCustomer;

public class UpdateCustomerValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid().ToString(), "Jane", "jane@autogarage.com", "41", "999999999");

        var result = new UpdateCustomerValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_ShouldReturnNameRequiredMessage(string name)
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid().ToString(), name, "jane@autogarage.com", "41", "999999999");

        var result = new UpdateCustomerValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The name field is required.");
    }

    [Fact]
    public void Validate_InvalidEmail_ShouldReturnEmailFormatMessage()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid().ToString(), "Jane", "jane@", "41", "999999999");

        var result = new UpdateCustomerValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The provided email format is invalid.");
    }

    [Fact]
    public void Validate_EmptyDdd_ShouldReturnDddRequiredMessage()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid().ToString(), "Jane", "jane@autogarage.com", "", "999999999");

        var result = new UpdateCustomerValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The ddd field is required.");
    }

    [Fact]
    public void Validate_EmptyPhone_ShouldReturnPhoneRequiredMessage()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid().ToString(), "Jane", "jane@autogarage.com", "41", "");

        var result = new UpdateCustomerValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The phone field is required.");
    }
}
