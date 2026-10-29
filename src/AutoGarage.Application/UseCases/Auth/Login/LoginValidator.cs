using AutoGarage.Application.Commands.Auth;
using FluentValidation;

namespace AutoGarage.Application.UseCases.Auth.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(login => login.Email)
            .NotEmpty().WithMessage("The email field is required.")
            .EmailAddress().WithMessage("The provided email format is invalid.");

        RuleFor(login => login.Password)
            .NotEmpty().WithMessage("The password field is required.");
    }
}
