using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders;

public class RemoveServiceFromServiceOrderValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankItemId_ShouldBeInvalid(string? itemId)
    {
        var command = new RemoveServiceFromServiceOrderCommand(Guid.NewGuid().ToString(), itemId!, Guid.NewGuid().ToString());

        var result = new RemoveServiceFromServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_FilledFields_ShouldBeValid()
    {
        var command = new RemoveServiceFromServiceOrderCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        var result = new RemoveServiceFromServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
