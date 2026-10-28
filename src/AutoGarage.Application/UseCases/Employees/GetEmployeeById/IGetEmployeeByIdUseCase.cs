using AutoGarage.Communication.Responses.Employees;

namespace AutoGarage.Application.UseCases.Employees.GetEmployeeById;

public interface IGetEmployeeByIdUseCase
{
    Task<EmployeeResponse> Handle(string id, CancellationToken cancellationToken);
}
