using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Domain.Entities;

public class ServiceOrderStatusHistory
{
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public ServiceOrderEventType EventType { get; private set; }
    public ServiceOrderStatus? FromStatus { get; private set; }
    public ServiceOrderStatus? ToStatus { get; private set; }
    public Guid? ItemId { get; private set; }
    public ServiceOrderItemDirection? ItemDirection { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected ServiceOrderStatusHistory() { }

    private ServiceOrderStatusHistory(
        Guid serviceOrderId,
        ServiceOrderEventType eventType,
        ServiceOrderStatus? fromStatus,
        ServiceOrderStatus? toStatus,
        Guid? itemId,
        ServiceOrderItemDirection? itemDirection,
        Guid employeeId)
    {
        Id = Guid.NewGuid();
        ServiceOrderId = serviceOrderId;
        EventType = eventType;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ItemId = itemId;
        ItemDirection = itemDirection;
        EmployeeId = employeeId;
        CreatedAt = DateTime.UtcNow;
    }

    public static ServiceOrderStatusHistory ForStatusChange(Guid serviceOrderId, ServiceOrderStatus? from, ServiceOrderStatus to, Guid employeeId)
    {
        return new ServiceOrderStatusHistory(serviceOrderId, ServiceOrderEventType.STATUS_CHANGE, from, to, null, null, employeeId);
    }

    public static ServiceOrderStatusHistory ForItemChange(Guid serviceOrderId, ServiceOrderEventType eventType, Guid itemId, ServiceOrderItemDirection direction, Guid employeeId)
    {
        if (eventType is not (ServiceOrderEventType.ITEM_SERVICE or ServiceOrderEventType.ITEM_MATERIAL))
        {
            throw new ArgumentException("Item events must be ITEM_SERVICE or ITEM_MATERIAL.", nameof(eventType));
        }

        return new ServiceOrderStatusHistory(serviceOrderId, eventType, null, null, itemId, direction, employeeId);
    }
}
