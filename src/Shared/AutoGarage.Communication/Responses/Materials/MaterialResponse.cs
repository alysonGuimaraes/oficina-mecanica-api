using AutoGarage.Domain.Shared;

namespace AutoGarage.Communication.Responses.Materials;

public record MaterialResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int StockMinimum,
    EntityStatus Status
);
