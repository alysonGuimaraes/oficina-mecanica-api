using AutoGarage.Application.Commands.Vehicles;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Vehicles.TransferVehicleOwner;

public class TransferVehicleOwnerValidator : AbstractValidator<TransferVehicleOwnerCommand>
{
    public TransferVehicleOwnerValidator()
    {
        RuleFor(vehicle => vehicle.NewOwnerDocument)
            .NotEmpty().WithMessage("The newOwnerDocument field is required.");
    }
}
