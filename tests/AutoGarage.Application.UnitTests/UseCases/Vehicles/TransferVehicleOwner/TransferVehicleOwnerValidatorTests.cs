using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.UseCases.Vehicles.TransferVehicleOwner;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.TransferVehicleOwner;

public class TransferVehicleOwnerValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        var command = new TransferVehicleOwnerCommand(Guid.NewGuid().ToString(), "529.982.247-25");

        var result = new TransferVehicleOwnerValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyDocument_ShouldReturnDocumentRequiredMessage(string document)
    {
        var command = new TransferVehicleOwnerCommand(Guid.NewGuid().ToString(), document);

        var result = new TransferVehicleOwnerValidator().Validate(command);

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The newOwnerDocument field is required.");
    }
}
