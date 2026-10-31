using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders;

public class AddMaterialToServiceOrderValidator : AbstractValidator<AddMaterialToServiceOrderCommand>
{
    public AddMaterialToServiceOrderValidator()
    {
        RuleFor(command => command.MaterialId)
            .NotEmpty().WithMessage("The materialId field is required.");

        RuleFor(command => command.Quantity)
            .GreaterThan(0).WithMessage("The quantity field must be greater than zero.");

        RuleFor(command => command.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
