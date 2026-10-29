using AutoGarage.Application.Commands.Auth;
using AutoGarage.Application.Services.Authentication;
using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Auth;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Auth.Exceptions;
using AutoGarage.Domain.Repositories.Employees;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Application.UseCases.Auth.Login;

public class LoginUseCase : ILoginUseCase
{
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly PasswordEncripter _passwordEncripter;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IStringNormalizer _stringNormalizer;

    public LoginUseCase(IEmployeeReadOnlyRepository employeeReadOnlyRepository, PasswordEncripter passwordEncripter, IAccessTokenGenerator accessTokenGenerator, IStringNormalizer stringNormalizer)
    {
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _passwordEncripter = passwordEncripter;
        _accessTokenGenerator = accessTokenGenerator;
        _stringNormalizer = stringNormalizer;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        command = Normalize(command);

        Validate(command);

        var employee = await _employeeReadOnlyRepository.GetByEmail(command.Email, cancellationToken);
        if (employee is null || employee.Status == EntityStatus.INACTIVE)
        {
            throw new InvalidCredentialsException();
        }

        if (!_passwordEncripter.Verify(command.Password, employee.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var (token, expiresAt) = _accessTokenGenerator.Generate(employee);

        return new LoginResponse(token, expiresAt);
    }

    // A senha não é normalizada: espaços podem fazer parte dela.
    private LoginCommand Normalize(LoginCommand command) =>
        command with
        {
            Email = _stringNormalizer.Normalize(command.Email).ToUpperInvariant()
        };

    private static void Validate(LoginCommand command)
    {
        var validator = new LoginValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
