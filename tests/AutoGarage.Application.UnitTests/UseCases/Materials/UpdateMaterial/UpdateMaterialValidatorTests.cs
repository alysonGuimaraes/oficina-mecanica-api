using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.UseCases.Materials.UpdateMaterial;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.UpdateMaterial;

public class UpdateMaterialValidatorTests
{
    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(name: ""));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The name field is required.");
    }

    [Fact]
    public void Validate_NameLongerThan150_ShouldFail()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(name: new string('A', 151)));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The name field must have at most 150 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1)]
    public void Validate_PriceNotGreaterThanZero_ShouldFail(decimal price)
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(price: price));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must be greater than zero.");
    }

    [Fact]
    public void Validate_PriceWithMoreThanTwoDecimals_ShouldFail()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(price: 10.005m));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must have at most 2 decimal places.");
    }

    [Fact]
    public void Validate_PriceAboveNumericLimit_ShouldFail()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(price: 100000000m));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must be less than 100000000.");
    }

    [Fact]
    public void Validate_NegativeStockMinimum_ShouldFail()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(stockMinimum: -1));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The stockMinimum field must not be negative.");
    }

    [Fact]
    public void Validate_DescriptionLongerThan500_ShouldFail()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(description: new string('A', 501)));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The description field must have at most 500 characters.");
    }

    [Fact]
    public void Validate_NullDescription_ShouldFail()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(description: null));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The description field is required.");
    }

    [Fact]
    public void Validate_BlankDescription_ShouldPass()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand(description: "   "));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var result = new UpdateMaterialValidator().Validate(CreateCommand());

        result.IsValid.ShouldBeTrue();
    }

    private static UpdateMaterialCommand CreateCommand(
        string name = "PARAFUSO",
        string? description = "Sextavado",
        decimal price = 2.50m,
        int stockMinimum = 3) =>
        new(Guid.NewGuid().ToString(), name, description, price, stockMinimum);
}
