using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.UseCases.Vehicles.CreateVehicle;
using AutoGarage.Domain.Enums.Vehicles;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Vehicles.CreateVehicle;

public class CreateVehicleValidatorTests
{
    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        var result = new CreateVehicleValidator().Validate(CreateCommand());

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmptyBrand_ShouldReturnBrandRequiredMessage()
    {
        var result = new CreateVehicleValidator().Validate(CreateCommand(brand: ""));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The brand field is required.");
    }

    [Fact]
    public void Validate_BrandWithMoreThan50Characters_ShouldReturnMaximumLengthMessage()
    {
        var result = new CreateVehicleValidator().Validate(CreateCommand(brand: new string('A', 51)));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The brand field must have at most 50 characters.");
    }

    [Fact]
    public void Validate_YearBeforeMinimum_ShouldReturnYearRangeMessage()
    {
        var result = new CreateVehicleValidator().Validate(CreateCommand(year: 1899));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain($"The year field must be between 1900 and {DateTime.UtcNow.Year + 1}.");
    }

    [Fact]
    public void Validate_YearAfterNextYear_ShouldReturnYearRangeMessage()
    {
        var result = new CreateVehicleValidator().Validate(CreateCommand(year: DateTime.UtcNow.Year + 2));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain($"The year field must be between 1900 and {DateTime.UtcNow.Year + 1}.");
    }

    [Fact]
    public void Validate_InvalidVehicleType_ShouldReturnVehicleTypeMessage()
    {
        var result = new CreateVehicleValidator().Validate(CreateCommand(vehicleType: (VehicleType)99));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The provided vehicle type is invalid.");
    }

    [Fact]
    public void Validate_EmptyPlate_ShouldReturnPlateRequiredMessage()
    {
        var result = new CreateVehicleValidator().Validate(CreateCommand(plate: ""));

        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The plate field is required.");
    }

    private static CreateVehicleCommand CreateCommand(
        string brand = "gol",
        string plate = "ABC1D23",
        int year = 2020,
        VehicleType vehicleType = VehicleType.CAR)
    {
        return new CreateVehicleCommand(brand, "1.0", plate, year, vehicleType, Guid.NewGuid().ToString());
    }
}
