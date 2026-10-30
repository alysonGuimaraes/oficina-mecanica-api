namespace AutoGarage.Application.Commands.Materials;

public record UpdateMaterialCommand(
    string Id,
    string Name,
    string? Description,
    decimal Price,
    int StockMinimum
);
