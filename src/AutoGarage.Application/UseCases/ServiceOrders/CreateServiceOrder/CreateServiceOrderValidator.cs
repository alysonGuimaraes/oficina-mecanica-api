using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders.CreateServiceOrder;

public class CreateServiceOrderValidator : AbstractValidator<CreateServiceOrderCommand>
{
    public CreateServiceOrderValidator()
    {
        RuleFor(order => order.CustomerId)
            .NotEmpty().WithMessage("The customerId field is required.");

        RuleFor(order => order.VehicleId)
            .NotEmpty().WithMessage("The vehicleId field is required.");

        RuleFor(order => order.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
