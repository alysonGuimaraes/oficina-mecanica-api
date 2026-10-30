using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Vehicles;

public class PlateTests
{
    [Fact]
    public void Plate_OldFormatWithHyphenAndLowerCase_ShouldNormalizeAndSetOldType()
    {
        var plate = new Plate("abc-1234");

        plate.Value.ShouldBe("ABC1234");
        plate.Type.ShouldBe(PlateType.OLD);
    }

    [Fact]
    public void Plate_MercosulFormatWithSpaceAndLowerCase_ShouldNormalizeAndSetMercosulType()
    {
        var plate = new Plate("abc 1d23");

        plate.Value.ShouldBe("ABC1D23");
        plate.Type.ShouldBe(PlateType.MERCOSUL);
    }

    [Theory]
    [InlineData("ABC12345")]
    [InlineData("AB1234")]
    [InlineData("ABCD123")]
    [InlineData("ABC12D3")]
    [InlineData("1BC1234")]
    [InlineData("")]
    public void Plate_InvalidFormat_ShouldThrowInvalidPlateException(string value)
    {
        var exception = Should.Throw<InvalidPlateException>(() => new Plate(value));

        exception.ErrorType.ShouldBe("invalid-plate");
        exception.ShouldBeAssignableTo<BadRequestDomainException>();
    }
}
