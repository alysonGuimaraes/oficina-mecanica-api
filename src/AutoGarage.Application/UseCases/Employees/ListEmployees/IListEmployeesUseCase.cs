using AutoGarage.Communication.Responses.Employees;

namespace AutoGarage.Application.UseCases.Employees.ListEmployees;

public interface IListEmployeesUseCase
{
    Task<IReadOnlyList<EmployeeResponse>> Handle(CancellationToken cancellationToken);
}
