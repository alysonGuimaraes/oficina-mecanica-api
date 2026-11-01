using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Communication.Responses.ServiceOrders;

public record ServiceOrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    ServiceOrderStatus Status,
    decimal? BudgetTotal,
    DateTime CreatedAt
);
