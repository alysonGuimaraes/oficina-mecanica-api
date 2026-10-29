namespace AutoGarage.Application.Commands.Employees;

public record UpdateEmployeeStatusCommand(
    string Id,
    string Status
);
