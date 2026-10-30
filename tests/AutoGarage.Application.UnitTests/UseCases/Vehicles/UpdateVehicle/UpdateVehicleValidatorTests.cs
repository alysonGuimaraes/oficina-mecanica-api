using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.UseCases.Vehicles.UpdateVehicle;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.UpdateVehicle;

public class UpdateVehicleValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand());

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmptyBrand_ShouldReturnBrandRequiredMessage()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand(brand: ""));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The brand field is required.");
    }

    [Fact]
    public void Validate_BrandWithMoreThan50Characters_ShouldReturnMaximumLengthMessage()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand(brand: new string('A', 51)));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The brand field must have at most 50 characters.");
    }

    [Fact]
    public void Validate_EmptyModel_ShouldReturnModelRequiredMessage()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand(model: ""));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The model field is required.");
    }

    [Fact]
    public void Validate_YearBeforeMinimum_ShouldReturnYearRangeMessage()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand(year: 1899));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain($"The year field must be between 1900 and {DateTime.UtcNow.Year + 1}.");
    }

    [Fact]
    public void Validate_YearAfterNextYear_ShouldReturnYearRangeMessage()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand(year: DateTime.UtcNow.Year + 2));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain($"The year field must be between 1900 and {DateTime.UtcNow.Year + 1}.");
    }

    [Fact]
    public void Validate_EmptyPlate_ShouldReturnPlateRequiredMessage()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand(plate: ""));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The plate field is required.");
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldReturnCustomerIdRequiredMessage()
    {
        var result = new UpdateVehicleValidator().Validate(CreateCommand(customerId: ""));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The customerId field is required.");
    }

    private static UpdateVehicleCommand CreateCommand(
        string brand = "gol",
        string model = "1.0",
        string plate = "ABC1D23",
        int year = 2020,
        string? customerId = null)
    {
        return new UpdateVehicleCommand(Guid.NewGuid().ToString(), brand, model, plate, year, customerId ?? Guid.NewGuid().ToString());
    }
}
