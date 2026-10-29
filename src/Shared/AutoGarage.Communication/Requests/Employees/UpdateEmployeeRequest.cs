using AutoGarage.Domain.Enums.Employees;

namespace AutoGarage.Communication.Requests.Employees;

public record UpdateEmployeeRequest(
    string Name,
    string Email,
    EmployeeRole Role
);
