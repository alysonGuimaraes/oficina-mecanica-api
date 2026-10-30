using AutoGarage.Application.Commands.Services;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Services.CreateService;

public class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(service => service.Name)
            .NotEmpty().WithMessage("The name field is required.")
            .MaximumLength(150).WithMessage("The name field must have at most 150 characters.");

        RuleFor(service => service.Description)
            .MaximumLength(500).WithMessage("The description field must have at most 500 characters.");

        RuleFor(service => service.Price)
            .GreaterThan(0).WithMessage("The price field must be greater than zero.")
            .Must(price => decimal.Round(price, 2) == price).WithMessage("The price field must have at most 2 decimal places.")
            .LessThan(100000000m).WithMessage("The price field must be less than 100000000.");
    }
}
