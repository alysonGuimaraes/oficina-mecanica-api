using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CompleteServiceOrder;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.CompleteServiceOrder;

public class CompleteServiceOrderValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankObservation_ShouldBeInvalid(string? observation)
    {
        var command = new CompleteServiceOrderCommand(Guid.NewGuid().ToString(), observation!, Guid.NewGuid().ToString());

        var result = new CompleteServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_ObservationAboveLimit_ShouldBeInvalid()
    {
        var command = new CompleteServiceOrderCommand(Guid.NewGuid().ToString(), new string('x', 501), Guid.NewGuid().ToString());

        var result = new CompleteServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_ObservationWithinLimitOnceTrimmed_ShouldBeValid()
    {
        var command = new CompleteServiceOrderCommand(Guid.NewGuid().ToString(), "  " + new string('x', 500) + "  ", Guid.NewGuid().ToString());

        var result = new CompleteServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankEmployeeId_ShouldBeInvalid(string? employeeId)
    {
        var command = new CompleteServiceOrderCommand(Guid.NewGuid().ToString(), "Sem reparo", employeeId!);

        var result = new CompleteServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_ObservationAtLimitAndFilledEmployee_ShouldBeValid()
    {
        var command = new CompleteServiceOrderCommand(Guid.NewGuid().ToString(), new string('x', 500), Guid.NewGuid().ToString());

        var result = new CompleteServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
