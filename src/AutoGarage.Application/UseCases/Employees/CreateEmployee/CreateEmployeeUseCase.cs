using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Communication.Requests.Employees;
using AutoGarage.Communication.Responses.Employees;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Employees;
using MapsterMapper;
using System.Text.RegularExpressions;

namespace AutoGarage.Application.UseCases.Employees.CreateEmployee;

public class CreateEmployeeUseCase : ICreateEmployeeUseCase
{
    private static readonly Regex MultipleWhitespace = new(@"\s+");

    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IEmployeeWriteOnlyRepository _employeeWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly PasswordEncripter _passwordEncripter;
    private readonly IMapper _mapper;

    public CreateEmployeeUseCase(IEmployeeReadOnlyRepository employeeReadOnlyRepository, IEmployeeWriteOnlyRepository employeeWriteOnlyRepository, IUnitWork workUnit, PasswordEncripter passwordEncripter, IMapper mapper)
    {
        _passwordEncripter = passwordEncripter;
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _employeeWriteOnlyRepository = employeeWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        command = Normalize(command);

        await Validate(command);

        var hasEmail = await _employeeReadOnlyRepository.ExistsByEmail(command.Email, cancellationToken);
        if (hasEmail)
        {
            throw new DuplicateEmailException(command.Email);
        }

        var passwordHash = _passwordEncripter.Encrypt(command.Password);

        var employee = Employee.Create(command.Name, command.Email, passwordHash, command.Role);

        await _employeeWriteOnlyRepository.Add(employee, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<EmployeeResponse>(employee);
    }

    // A senha não é normalizada: espaços podem fazer parte dela.
    private static CreateEmployeeCommand Normalize(CreateEmployeeCommand command) =>
        command with
        {
            Name = NormalizeWhitespace(command.Name),
            Email = NormalizeWhitespace(command.Email).ToUpperInvariant()
        };

    private static string NormalizeWhitespace(string? value) =>
        MultipleWhitespace.Replace(value ?? string.Empty, " ").Trim();

    private static async Task Validate(CreateEmployeeCommand command)
    {
        var validator = new CreateEmployeeValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}

