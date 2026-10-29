using AutoGarage.Application.Commands.Customers;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Customers.CreateCustomer;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(customer => customer.Name)
            .NotEmpty().WithMessage("The name field is required.");

        RuleFor(customer => customer.Document)
            .NotEmpty().WithMessage("The document field is required.");

        RuleFor(customer => customer.Email)
            .NotEmpty().WithMessage("The email field is required.")
            .EmailAddress().WithMessage("The provided email format is invalid.");

        RuleFor(customer => customer.Ddd)
            .NotEmpty().WithMessage("The ddd field is required.");

        RuleFor(customer => customer.Phone)
            .NotEmpty().WithMessage("The phone field is required.");
    }
}
