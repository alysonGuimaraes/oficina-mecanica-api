namespace AutoGarage.Communication.Requests.Materials;

public record CreateMaterialRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockMinimum
);
