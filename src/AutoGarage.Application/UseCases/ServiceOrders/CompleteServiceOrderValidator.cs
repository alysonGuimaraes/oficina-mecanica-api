using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders;

public class CompleteServiceOrderValidator : AbstractValidator<CompleteServiceOrderCommand>
{
    public CompleteServiceOrderValidator()
    {
        RuleFor(command => command.Observation)
            .NotEmpty().WithMessage("The observation field is required.")
            .Must(observation => observation is null || observation.Trim().Length <= 500)
            .WithMessage("The observation field must have at most 500 characters.");

        RuleFor(command => command.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
