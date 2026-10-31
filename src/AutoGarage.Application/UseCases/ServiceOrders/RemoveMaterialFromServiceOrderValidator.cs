using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders;

public class RemoveMaterialFromServiceOrderValidator : AbstractValidator<RemoveMaterialFromServiceOrderCommand>
{
    public RemoveMaterialFromServiceOrderValidator()
    {
        RuleFor(command => command.ItemId)
            .NotEmpty().WithMessage("The itemId field is required.");

        RuleFor(command => command.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
