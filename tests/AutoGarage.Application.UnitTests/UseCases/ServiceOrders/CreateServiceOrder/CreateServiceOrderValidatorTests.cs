using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CreateServiceOrder;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.CreateServiceOrder;

public class CreateServiceOrderValidatorTests
{
    private const string FilledId = "7d7b1d1e-2b1a-4a7e-9a55-0c6a5b1d2e11";

    [Fact]
    public void Validate_AllFieldsFilled_ShouldBeValid()
    {
        var command = new CreateServiceOrderCommand(FilledId, FilledId, FilledId);

        var result = new CreateServiceOrderValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldReturnRequiredMessage()
    {
        var command = new CreateServiceOrderCommand("", FilledId, FilledId);

        var result = new CreateServiceOrderValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The customerId field is required.");
    }

    [Fact]
    public void Validate_EmptyVehicleId_ShouldReturnRequiredMessage()
    {
        var command = new CreateServiceOrderCommand(FilledId, "", FilledId);

        var result = new CreateServiceOrderValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The vehicleId field is required.");
    }

    [Fact]
    public void Validate_EmptyEmployeeId_ShouldReturnRequiredMessage()
    {
        var command = new CreateServiceOrderCommand(FilledId, FilledId, "");

        var result = new CreateServiceOrderValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The employeeId field is required.");
    }
}
