using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Communication.Responses.ServiceOrders;

public record ServiceOrderStatusHistoryResponse(
    Guid Id,
    ServiceOrderEventType EventType,
    ServiceOrderStatus? FromStatus,
    ServiceOrderStatus? ToStatus,
    Guid? ItemId,
    ServiceOrderItemDirection? ItemDirection,
    Guid EmployeeId,
    DateTime CreatedAt
);
