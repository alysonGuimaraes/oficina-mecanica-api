namespace AutoGarage.Application.Commands.Materials;

public record CreateMaterialCommand(
    string Name,
    string? Description,
    decimal Price,
    int StockMinimum
);
