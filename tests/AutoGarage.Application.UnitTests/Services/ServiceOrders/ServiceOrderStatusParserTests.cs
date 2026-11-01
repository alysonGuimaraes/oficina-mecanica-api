using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Domain.Enums.ServiceOrders;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.ServiceOrders;

public class ServiceOrderStatusParserTests
{
    [Theory]
    [InlineData("in_diagnosis", ServiceOrderStatus.IN_DIAGNOSIS)]
    [InlineData(" Delivered ", ServiceOrderStatus.DELIVERED)]
    public void TryParse_ValidText_ShouldReturnTrueAndStatus(string value, ServiceOrderStatus expected)
    {
        var result = ServiceOrderStatusParser.TryParse(value, out var status);

        result.ShouldBeTrue();
        status.ShouldBe(expected);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("1")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_InvalidText_ShouldReturnFalse(string? value)
    {
        var result = ServiceOrderStatusParser.TryParse(value, out _);

        result.ShouldBeFalse();
    }
}
