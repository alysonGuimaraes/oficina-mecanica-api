using AutoGarage.Application.Services.Status;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.Status;

public class EntityStatusParserTests
{
    [Theory]
    [InlineData("ACTIVE", EntityStatus.ACTIVE)]
    [InlineData("inactive", EntityStatus.INACTIVE)]
    [InlineData("  Inactive  ", EntityStatus.INACTIVE)]
    public void TryParse_ValidText_ShouldReturnTrueAndStatus(string value, EntityStatus expected)
    {
        var result = EntityStatusParser.TryParse(value, out var status);

        result.ShouldBeTrue();
        status.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("FOO")]
    public void TryParse_InvalidText_ShouldReturnFalse(string? value)
    {
        var result = EntityStatusParser.TryParse(value, out _);

        result.ShouldBeFalse();
    }
}
