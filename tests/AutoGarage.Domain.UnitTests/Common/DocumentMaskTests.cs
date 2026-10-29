using AutoGarage.Domain.Common;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Common;

public class DocumentMaskTests
{
    [Fact]
    public void Mask_Cpf_ShouldShowOnlyFirstTwoDigits()
    {
        DocumentMask.Mask("52998224725").ShouldBe("52XXXXXXXXX");
    }

    [Fact]
    public void Mask_AlphanumericCnpj_ShouldShowOnlyFirstTwoCharacters()
    {
        DocumentMask.Mask("12ABC34501DE35").ShouldBe("12XXXXXXXXXXXX");
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("AB")]
    public void Mask_TwoCharactersOrLess_ShouldReturnValueUnchanged(string value)
    {
        DocumentMask.Mask(value).ShouldBe(value);
    }
}
