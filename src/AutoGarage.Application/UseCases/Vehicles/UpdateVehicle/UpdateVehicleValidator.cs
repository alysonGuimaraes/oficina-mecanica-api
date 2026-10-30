using AutoGarage.Application.Commands.Vehicles;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Vehicles.UpdateVehicle;

public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleValidator()
    {
        RuleFor(vehicle => vehicle.Brand)
            .NotEmpty().WithMessage("The brand field is required.")
            .MaximumLength(50).WithMessage("The brand field must have at most 50 characters.");

        RuleFor(vehicle => vehicle.Model)
            .NotEmpty().WithMessage("The model field is required.")
            .MaximumLength(50).WithMessage("The model field must have at most 50 characters.");

        RuleFor(vehicle => vehicle.Plate)
            .NotEmpty().WithMessage("The plate field is required.");

        RuleFor(vehicle => vehicle.Year)
            .Must(year => year >= 1900 && year <= DateTime.UtcNow.Year + 1)
            .WithMessage(_ => $"The year field must be between 1900 and {DateTime.UtcNow.Year + 1}.");
    }
}
