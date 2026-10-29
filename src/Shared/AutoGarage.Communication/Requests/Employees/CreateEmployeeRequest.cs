using AutoGarage.Domain.Enums.Employees;

namespace AutoGarage.Communication.Requests.Employees;

public record CreateEmployeeRequest(
    string Name,
    string Email,
    string Password,
    EmployeeRole Role
);
