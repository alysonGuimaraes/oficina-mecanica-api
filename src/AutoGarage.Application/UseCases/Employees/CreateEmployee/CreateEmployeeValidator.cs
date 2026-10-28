using AutoGarage.Application.Commands.Employees;
using AutoGarage.Communication.Requests.Employees;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Employees.CreateEmployee;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(employee => employee.Name)
            .NotEmpty().WithMessage("The name field is required.");

        RuleFor(employee => employee.Email)
            .NotEmpty().WithMessage("The email field is required.")
            .EmailAddress().WithMessage("The provided email format is invalid.");

        RuleFor(employee => employee.Password)
            .NotEmpty().WithMessage("The password field is required.")
            .MinimumLength(6).WithMessage("The password must be at least 6 characters long.");

        RuleFor(employee => employee.Role)
            .IsInEnum().WithMessage("The provided role is invalid. Allowed roles are MECHANIC or ATTENDANT.");
    }
}
