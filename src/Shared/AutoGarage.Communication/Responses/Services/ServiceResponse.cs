using AutoGarage.Domain.Shared;

namespace AutoGarage.Communication.Responses.Services;

public record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    EntityStatus Status
);
