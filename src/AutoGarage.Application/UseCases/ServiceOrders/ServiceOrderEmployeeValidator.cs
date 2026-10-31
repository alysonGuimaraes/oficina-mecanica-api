using AutoGarage.Application.Commands.ServiceOrders;
using FluentValidation;

namespace AutoGarage.Application.UseCases.ServiceOrders;

public class ServiceOrderEmployeeValidator : AbstractValidator<ServiceOrderEmployeeCommand>
{
    public ServiceOrderEmployeeValidator()
    {
        RuleFor(command => command.EmployeeId)
            .NotEmpty().WithMessage("The employeeId field is required.");
    }
}
