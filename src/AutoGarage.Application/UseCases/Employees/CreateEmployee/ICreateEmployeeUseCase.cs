using AutoGarage.Application.Commands.Employees;
using AutoGarage.Communication.Responses.Employees;

namespace AutoGarage.Application.UseCases.Employees.CreateEmployee;

public interface ICreateEmployeeUseCase
{
    public Task<EmployeeResponse> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken);
}
