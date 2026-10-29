using AutoGarage.Application.Commands.Employees;
using AutoGarage.Communication.Responses.Employees;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Employees;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Employees.UpdateEmployeeStatus;

public class UpdateEmployeeStatusUseCase : IUpdateEmployeeStatusUseCase
{
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IEmployeeWriteOnlyRepository _employeeWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public UpdateEmployeeStatusUseCase(IEmployeeReadOnlyRepository employeeReadOnlyRepository, IEmployeeWriteOnlyRepository employeeWriteOnlyRepository, IUnitWork workUnit, IMapper mapper)
    {
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _employeeWriteOnlyRepository = employeeWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse> Handle(UpdateEmployeeStatusCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var employeeId))
        {
            throw new InvalidEmployeeIdException();
        }

        var status = ParseStatus(command.Status);

        var employee = await _employeeReadOnlyRepository.GetById(employeeId, cancellationToken)
            ?? throw new EmployeeNotFoundException(employeeId);

        if (status == EntityStatus.ACTIVE)
        {
            employee.Activate();
        }
        else
        {
            employee.Deactivate();
        }

        await _employeeWriteOnlyRepository.Update(employee, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<EmployeeResponse>(employee);
    }

    // Enum.TryParse aceitaria números ("1", "0"); por isso a comparação é explícita.
    private static EntityStatus ParseStatus(string value) =>
        value.ToUpperInvariant() switch
        {
            "ACTIVE" => EntityStatus.ACTIVE,
            "INACTIVE" => EntityStatus.INACTIVE,
            _ => throw new InvalidEmployeeStatusException()
        };
}
