namespace AutoGarage.Communication.Responses.ServiceOrders;

public record ServiceOrderMaterialItemResponse(
    Guid Id,
    Guid MaterialId,
    int Quantity,
    decimal UnitPrice
);
