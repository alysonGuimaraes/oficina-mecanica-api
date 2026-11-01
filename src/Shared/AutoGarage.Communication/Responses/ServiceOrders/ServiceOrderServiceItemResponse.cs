namespace AutoGarage.Communication.Responses.ServiceOrders;

public record ServiceOrderServiceItemResponse(
    Guid Id,
    Guid ServiceId,
    decimal Price
);
