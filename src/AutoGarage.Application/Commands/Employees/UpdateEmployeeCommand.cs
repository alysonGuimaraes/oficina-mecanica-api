using AutoGarage.Domain.Enums.Employees;

namespace AutoGarage.Application.Commands.Employees;

public record UpdateEmployeeCommand(
    string Id,
    string Name,
    string Email,
    EmployeeRole Role
);
