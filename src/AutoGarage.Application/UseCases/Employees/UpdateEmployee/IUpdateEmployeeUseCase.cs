using AutoGarage.Application.Commands.Employees;
using AutoGarage.Communication.Responses.Employees;

namespace AutoGarage.Application.UseCases.Employees.UpdateEmployee;

public interface IUpdateEmployeeUseCase
{
    Task<EmployeeResponse> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken);
}
