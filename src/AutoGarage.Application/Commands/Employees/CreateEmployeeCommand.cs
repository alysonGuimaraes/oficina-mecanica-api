using AutoGarage.Domain.Enums.Employees;

namespace AutoGarage.Application.Commands.Employees;

public record CreateEmployeeCommand(
    string Name,
    string Email,
    string Password,
    EmployeeRole Role
);
