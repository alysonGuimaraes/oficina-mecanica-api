using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders;

public class RemoveServiceFromServiceOrderValidator : AbstractValidator<RemoveServiceFromServiceOrderCommand>
{
    public RemoveServiceFromServiceOrderValidator()
    {
        RuleFor(command => command.ItemId)
            .NotEmpty().WithMessage("The itemId field is required.");

        RuleFor(command => command.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
