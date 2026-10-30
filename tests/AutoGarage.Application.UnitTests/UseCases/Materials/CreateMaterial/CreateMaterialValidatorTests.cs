using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.UseCases.Materials.CreateMaterial;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.CreateMaterial;

public class CreateMaterialValidatorTests
{
    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("", "Sextavado", 2.50m, 3));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The name field is required.");
    }

    [Fact]
    public void Validate_NameLongerThan150_ShouldFail()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand(new string('A', 151), null, 2.50m, 3));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The name field must have at most 150 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1)]
    public void Validate_PriceNotGreaterThanZero_ShouldFail(decimal price)
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("PARAFUSO", null, price, 3));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must be greater than zero.");
    }

    [Fact]
    public void Validate_PriceWithMoreThanTwoDecimals_ShouldFail()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("PARAFUSO", null, 10.005m, 3));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must have at most 2 decimal places.");
    }

    [Fact]
    public void Validate_PriceAboveNumericLimit_ShouldFail()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("PARAFUSO", null, 100000000m, 3));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must be less than 100000000.");
    }

    [Fact]
    public void Validate_NegativeStockMinimum_ShouldFail()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("PARAFUSO", null, 2.50m, -1));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The stockMinimum field must not be negative.");
    }

    [Fact]
    public void Validate_DescriptionLongerThan500_ShouldFail()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("PARAFUSO", new string('A', 501), 2.50m, 3));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The description field must have at most 500 characters.");
    }

    [Fact]
    public void Validate_ZeroStockMinimum_ShouldPass()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("PARAFUSO", null, 2.50m, 0));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var result = new CreateMaterialValidator().Validate(new CreateMaterialCommand("PARAFUSO", "Sextavado", 2.50m, 3));

        result.IsValid.ShouldBeTrue();
    }
}
