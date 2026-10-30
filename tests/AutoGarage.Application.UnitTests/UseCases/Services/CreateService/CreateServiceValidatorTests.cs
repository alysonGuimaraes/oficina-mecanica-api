using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.UseCases.Services.CreateService;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Services.CreateService;

public class CreateServiceValidatorTests
{
    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var result = new CreateServiceValidator().Validate(new CreateServiceCommand("", null, 10m));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The name field is required.");
    }

    [Fact]
    public void Validate_NameLongerThan150_ShouldFail()
    {
        var result = new CreateServiceValidator().Validate(new CreateServiceCommand(new string('A', 151), null, 10m));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The name field must have at most 150 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PriceNotPositive_ShouldFail(decimal price)
    {
        var result = new CreateServiceValidator().Validate(new CreateServiceCommand("ALINHAMENTO", null, price));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must be greater than zero.");
    }

    [Fact]
    public void Validate_PriceWithMoreThanTwoDecimals_ShouldFail()
    {
        var result = new CreateServiceValidator().Validate(new CreateServiceCommand("ALINHAMENTO", null, 10.005m));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must have at most 2 decimal places.");
    }

    [Fact]
    public void Validate_PriceAboveNumericLimit_ShouldFail()
    {
        var result = new CreateServiceValidator().Validate(new CreateServiceCommand("ALINHAMENTO", null, 100000000m));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The price field must be less than 100000000.");
    }

    [Fact]
    public void Validate_DescriptionLongerThan500_ShouldFail()
    {
        var result = new CreateServiceValidator().Validate(new CreateServiceCommand("ALINHAMENTO", new string('A', 501), 10m));

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The description field must have at most 500 characters.");
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var result = new CreateServiceValidator().Validate(new CreateServiceCommand("ALINHAMENTO", "Com balanceamento", 80m));

        result.IsValid.ShouldBeTrue();
    }
}
