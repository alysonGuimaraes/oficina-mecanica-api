using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders;

public class AddServiceToServiceOrderValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankServiceId_ShouldBeInvalid(string? serviceId)
    {
        var command = new AddServiceToServiceOrderCommand(Guid.NewGuid().ToString(), serviceId!, Guid.NewGuid().ToString());

        var result = new AddServiceToServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_FilledFields_ShouldBeValid()
    {
        var command = new AddServiceToServiceOrderCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        var result = new AddServiceToServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
