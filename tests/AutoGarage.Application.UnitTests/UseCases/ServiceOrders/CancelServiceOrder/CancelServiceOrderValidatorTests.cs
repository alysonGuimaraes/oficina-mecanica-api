using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CancelServiceOrder;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.CancelServiceOrder;

public class CancelServiceOrderValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankReason_ShouldBeInvalid(string? reason)
    {
        var command = new CancelServiceOrderCommand(Guid.NewGuid().ToString(), reason!, Guid.NewGuid().ToString());

        var result = new CancelServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_ReasonAboveLimit_ShouldBeInvalid()
    {
        var command = new CancelServiceOrderCommand(Guid.NewGuid().ToString(), new string('x', 501), Guid.NewGuid().ToString());

        var result = new CancelServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_ReasonWithinLimitOnceTrimmed_ShouldBeValid()
    {
        var command = new CancelServiceOrderCommand(Guid.NewGuid().ToString(), "  " + new string('x', 500) + "  ", Guid.NewGuid().ToString());

        var result = new CancelServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankEmployeeId_ShouldBeInvalid(string? employeeId)
    {
        var command = new CancelServiceOrderCommand(Guid.NewGuid().ToString(), "Cliente desistiu", employeeId!);

        var result = new CancelServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_FilledFields_ShouldBeValid()
    {
        var command = new CancelServiceOrderCommand(Guid.NewGuid().ToString(), "Cliente desistiu", Guid.NewGuid().ToString());

        var result = new CancelServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
