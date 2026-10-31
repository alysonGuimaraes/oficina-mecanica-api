using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders;

public class ServiceOrderEmployeeValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankEmployeeId_ShouldBeInvalid(string? employeeId)
    {
        var command = new ServiceOrderEmployeeCommand(Guid.NewGuid().ToString(), employeeId!);

        var result = new ServiceOrderEmployeeValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validate_FilledEmployeeId_ShouldBeValid()
    {
        var command = new ServiceOrderEmployeeCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        var result = new ServiceOrderEmployeeValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
