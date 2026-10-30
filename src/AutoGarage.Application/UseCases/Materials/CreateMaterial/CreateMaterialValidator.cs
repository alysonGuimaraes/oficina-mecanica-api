using AutoGarage.Application.Commands.Materials;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Materials.CreateMaterial;

public class CreateMaterialValidator : AbstractValidator<CreateMaterialCommand>
{
    public CreateMaterialValidator()
    {
        RuleFor(material => material.Name)
            .NotEmpty().WithMessage("The name field is required.")
            .MaximumLength(150).WithMessage("The name field must have at most 150 characters.");

        RuleFor(material => material.Description)
            .MaximumLength(500).WithMessage("The description field must have at most 500 characters.");

        RuleFor(material => material.Price)
            .GreaterThan(0).WithMessage("The price field must be greater than zero.")
            .Must(price => decimal.Round(price, 2) == price).WithMessage("The price field must have at most 2 decimal places.")
            .LessThan(100000000m).WithMessage("The price field must be less than 100000000.");

        RuleFor(material => material.StockMinimum)
            .GreaterThanOrEqualTo(0).WithMessage("The stockMinimum field must not be negative.");
    }
}
