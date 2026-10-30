namespace AutoGarage.Communication.Requests.Materials;

public record UpdateMaterialRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockMinimum
);
