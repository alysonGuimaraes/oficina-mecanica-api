using AutoGarage.Application.Services.Normalization;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.Normalization;

public class StringNormalizerTests
{
    [Theory]
    [InlineData("  John   Mechanic  ", "John Mechanic")]
    [InlineData("John", "John")]
    [InlineData("  mixed Case   Text ", "mixed Case Text")]
    [InlineData(null, "")]
    public void Normalize_ShouldTrimCollapseSpacesAndKeepCase(string? input, string expected)
    {
        var normalizer = new StringNormalizer();

        normalizer.Normalize(input).ShouldBe(expected);
    }
}
