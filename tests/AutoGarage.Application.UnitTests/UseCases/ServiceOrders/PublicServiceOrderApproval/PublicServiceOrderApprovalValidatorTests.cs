using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.PublicServiceOrderApproval;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.ServiceOrders.PublicServiceOrderApproval;

public class PublicServiceOrderApprovalValidatorTests
{
    [Fact]
    public void Validate_NullApproved_ShouldFail()
    {
        var command = new PublicServiceOrderApprovalCommand(Guid.NewGuid().ToString(), null);

        var result = new PublicServiceOrderApprovalValidator().Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(e => e.ErrorMessage).ShouldContain("The approved field is required.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_TrueOrFalse_ShouldPass(bool approved)
    {
        var command = new PublicServiceOrderApprovalCommand(Guid.NewGuid().ToString(), approved);

        var result = new PublicServiceOrderApprovalValidator().Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
