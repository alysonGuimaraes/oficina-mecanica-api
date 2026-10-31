using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Communication.Responses.ServiceOrders;

public record ServiceOrderDetailResponse(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    ServiceOrderStatus Status,
    decimal? BudgetTotal,
    DateTime? BudgetSentAt,
    string? Observation,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ServiceOrderServiceItemResponse> Services,
    IReadOnlyList<ServiceOrderMaterialItemResponse> Materials,
    IReadOnlyList<ServiceOrderStatusHistoryResponse> History
);
