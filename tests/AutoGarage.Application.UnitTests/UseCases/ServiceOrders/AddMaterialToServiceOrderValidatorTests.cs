using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders;

public class AddMaterialToServiceOrderValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankMaterialId_ShouldBeInvalid(string? materialId)
    {
        var command = new AddMaterialToServiceOrderCommand(Guid.NewGuid().ToString(), materialId!, 2, Guid.NewGuid().ToString());

        var result = new AddMaterialToServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositiveQuantity_ShouldBeInvalid(int quantity)
    {
        var command = new AddMaterialToServiceOrderCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), quantity, Guid.NewGuid().ToString());

        var result = new AddMaterialToServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_FilledFields_ShouldBeValid()
    {
        var command = new AddMaterialToServiceOrderCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 2, Guid.NewGuid().ToString());

        var result = new AddMaterialToServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
