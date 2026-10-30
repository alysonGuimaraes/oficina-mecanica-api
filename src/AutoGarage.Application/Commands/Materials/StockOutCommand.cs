namespace AutoGarage.Application.Commands.Materials;

public record StockOutCommand(
    string MaterialId,
    int Quantity
);
