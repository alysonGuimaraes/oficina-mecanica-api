namespace AutoGarage.Application.Commands.Materials;

public record StockEntryCommand(
    string MaterialId,
    int Quantity
);
