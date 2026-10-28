using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Employees;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Employees;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Employees.CreateEmployee;

public class CreateEmployeeUseCase : ICreateEmployeeUseCase
{
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IEmployeeWriteOnlyRepository _employeeWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly PasswordEncripter _passwordEncripter;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public CreateEmployeeUseCase(IEmployeeReadOnlyRepository employeeReadOnlyRepository, IEmployeeWriteOnlyRepository employeeWriteOnlyRepository, IUnitWork workUnit, PasswordEncripter passwordEncripter, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _passwordEncripter = passwordEncripter;
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _employeeWriteOnlyRepository = employeeWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        request = Normalize(request);

        await Validate(request);

        var hasEmail = await _employeeReadOnlyRepository.ExistsByEmail(request.Email, cancellationToken);
        if (hasEmail)
        {
            throw new DuplicateEmailException(request.Email);
        }

        var passwordHash = _passwordEncripter.Encrypt(request.Password);

        var employee = Employee.Create(request.Name, request.Email, passwordHash, request.Role);

        await _employeeWriteOnlyRepository.Add(employee, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<EmployeeResponse>(employee);
    }

    // A senha não é normalizada: espaços podem fazer parte dela.
    private CreateEmployeeCommand Normalize(CreateEmployeeCommand command) =>
        command with
        {
            Name = _stringNormalizer.Normalize(command.Name),
            Email = _stringNormalizer.Normalize(command.Email).ToUpperInvariant()
        };

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
