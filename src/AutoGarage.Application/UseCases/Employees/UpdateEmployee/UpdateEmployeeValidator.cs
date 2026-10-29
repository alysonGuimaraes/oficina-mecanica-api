using AutoGarage.Application.Commands.Employees;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Employees.UpdateEmployee;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(employee => employee.Name)
            .NotEmpty().WithMessage("The name field is required.");

        RuleFor(employee => employee.Email)
            .NotEmpty().WithMessage("The email field is required.")
            .EmailAddress().WithMessage("The provided email format is invalid.");

        RuleFor(employee => employee.Role)
            .IsInEnum().WithMessage("The provided role is invalid. Allowed roles are MECHANIC or ATTENDANT.");
    }
}
