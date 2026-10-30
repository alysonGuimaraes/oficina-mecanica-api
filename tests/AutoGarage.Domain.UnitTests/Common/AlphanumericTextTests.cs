using AutoGarage.Domain.Common;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Common;

public class AlphanumericTextTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("12.abc.345/01de-35", "12ABC34501DE35")]
    [InlineData("abc-1d23", "ABC1D23")]
    [InlineData(null, "")]
    public void Hygienize_ShouldKeepOnlyUppercaseAlphanumericCharacters(string? value, string expected)
    {
        AlphanumericText.Hygienize(value).ShouldBe(expected);
    }
}
