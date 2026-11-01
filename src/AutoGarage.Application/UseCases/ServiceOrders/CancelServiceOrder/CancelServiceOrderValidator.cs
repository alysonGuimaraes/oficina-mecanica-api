using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders.CancelServiceOrder;

public class CancelServiceOrderValidator : AbstractValidator<CancelServiceOrderCommand>
{
    public CancelServiceOrderValidator()
    {
        RuleFor(command => command.Reason)
            .NotEmpty().WithMessage("The reason field is required.")
            .Must(reason => reason is null || reason.Trim().Length <= 500)
            .WithMessage("The reason field must have at most 500 characters.");

        RuleFor(command => command.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
