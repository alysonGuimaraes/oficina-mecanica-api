using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Communication.Responses.Employees;

public record EmployeeResponse(
    Guid Id,
    string Name,
    string Email,
    EmployeeRole Role,
    EntityStatus Status
);
