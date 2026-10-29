using AutoGarage.Application.Commands.Employees;
using AutoGarage.Communication.Responses.Employees;

namespace AutoGarage.Application.UseCases.Employees.UpdateEmployeeStatus;

public interface IUpdateEmployeeStatusUseCase
{
    Task<EmployeeResponse> Handle(UpdateEmployeeStatusCommand command, CancellationToken cancellationToken);
}
