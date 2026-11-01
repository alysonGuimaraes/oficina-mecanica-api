using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders.PublicServiceOrderApproval;

public class PublicServiceOrderApprovalValidator : AbstractValidator<PublicServiceOrderApprovalCommand>
{
    public PublicServiceOrderApprovalValidator()
    {
        RuleFor(c => c.Approved)
            .NotNull()
            .WithMessage("The approved field is required.");
    }
}
