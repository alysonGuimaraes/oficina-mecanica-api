using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders;

public class AddServiceToServiceOrderValidator : AbstractValidator<AddServiceToServiceOrderCommand>
{
    public AddServiceToServiceOrderValidator()
    {
        RuleFor(command => command.ServiceId)
            .NotEmpty().WithMessage("The serviceId field is required.");

        RuleFor(command => command.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
