namespace AutoGarage.Domain.Enums.ServiceOrders;

public enum ServiceOrderStatus
{
    RECEIVED = 1,
    IN_DIAGNOSIS = 2,
    AWAITING_APPROVAL = 3,
    AWAITING_MATERIALS = 4,
    IN_EXECUTION = 5,
    FINISHED = 6,
    CANCELLED = 7,
    DELIVERED = 8
}
