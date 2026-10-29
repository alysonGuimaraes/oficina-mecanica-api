using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Employees;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Employees;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Employees.UpdateEmployee;

public class UpdateEmployeeUseCase : IUpdateEmployeeUseCase
{
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IEmployeeWriteOnlyRepository _employeeWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public UpdateEmployeeUseCase(IEmployeeReadOnlyRepository employeeReadOnlyRepository, IEmployeeWriteOnlyRepository employeeWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _employeeWriteOnlyRepository = employeeWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var employeeId))
        {
            throw new InvalidEmployeeIdException();
        }

        command = Normalize(command);

        Validate(command);

        var employee = await _employeeReadOnlyRepository.GetById(employeeId, cancellationToken)
            ?? throw new EmployeeNotFoundException(employeeId);

        var hasEmailOfAnotherEmployee = await _employeeReadOnlyRepository
            .ExistsByEmailExcludingId(command.Email, employeeId, cancellationToken);
        if (hasEmailOfAnotherEmployee)
        {
            throw new DuplicateEmailException(command.Email);
        }

        employee.UpdateProfile(command.Name, command.Email, command.Role);

        await _employeeWriteOnlyRepository.Update(employee, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<EmployeeResponse>(employee);
    }

    private UpdateEmployeeCommand Normalize(UpdateEmployeeCommand command) =>
        command with
        {
            Name = _stringNormalizer.Normalize(command.Name),
            Email = _stringNormalizer.Normalize(command.Email).ToUpperInvariant()
        };

    private static void Validate(UpdateEmployeeCommand command)
    {
        var validator = new UpdateEmployeeValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
