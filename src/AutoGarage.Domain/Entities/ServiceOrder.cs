using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

namespace AutoGarage.Domain.Entities;

public class ServiceOrder
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public decimal? BudgetTotal { get; private set; }
    public DateTime? BudgetSentAt { get; private set; }
    public string? Observation { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected ServiceOrder() { }

    private ServiceOrder(Guid customerId, Guid vehicleId)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        VehicleId = vehicleId;
        Status = ServiceOrderStatus.RECEIVED;
        CreatedAt = DateTime.UtcNow;
    }

    public static ServiceOrder Create(Guid customerId, Guid vehicleId)
    {
        return new ServiceOrder(customerId, vehicleId);
    }

    // Transições sem dados extras. Orçamento, finalização, recusa e cancelamento têm método próprio.
    public ServiceOrderStatusHistory ChangeStatus(ServiceOrderStatus to, Guid employeeId)
    {
        if (to is ServiceOrderStatus.AWAITING_APPROVAL or ServiceOrderStatus.FINISHED or ServiceOrderStatus.CANCELLED)
        {
            throw new InvalidServiceOrderTransitionException(Status, to);
        }

        return MoveTo(to, employeeId);
    }

    public ServiceOrderStatusHistory SendBudget(decimal budgetTotal, Guid employeeId)
    {
        EnsureTransition(ServiceOrderStatus.AWAITING_APPROVAL);

        BudgetTotal = budgetTotal;
        BudgetSentAt = DateTime.UtcNow;

        return MoveTo(ServiceOrderStatus.AWAITING_APPROVAL, employeeId);
    }

    public ServiceOrderStatusHistory Complete(string observation, Guid employeeId)
    {
        if (Status is not (ServiceOrderStatus.IN_DIAGNOSIS or ServiceOrderStatus.IN_EXECUTION))
        {
            throw new InvalidServiceOrderTransitionException(Status, ServiceOrderStatus.FINISHED);
        }

        EnsureTransition(ServiceOrderStatus.FINISHED);

        Observation = observation;

        return MoveTo(ServiceOrderStatus.FINISHED, employeeId);
    }

    public ServiceOrderStatusHistory Refuse(Guid employeeId)
    {
        if (Status != ServiceOrderStatus.AWAITING_APPROVAL)
        {
            throw new InvalidServiceOrderTransitionException(Status, ServiceOrderStatus.FINISHED);
        }

        return MoveTo(ServiceOrderStatus.FINISHED, employeeId);
    }

    public ServiceOrderStatusHistory Cancel(string reason, Guid employeeId)
    {
        EnsureTransition(ServiceOrderStatus.CANCELLED);

        CancellationReason = reason;

        return MoveTo(ServiceOrderStatus.CANCELLED, employeeId);
    }

    public void EnsureItemsEditable()
    {
        if (Status != ServiceOrderStatus.IN_DIAGNOSIS)
        {
            throw new ServiceOrderNotEditableException();
        }
    }

    private void EnsureTransition(ServiceOrderStatus to)
    {
        if (!ServiceOrderTransitions.IsAllowed(Status, to))
        {
            throw new InvalidServiceOrderTransitionException(Status, to);
        }
    }

    private ServiceOrderStatusHistory MoveTo(ServiceOrderStatus to, Guid employeeId)
    {
        EnsureTransition(to);

        var from = Status;
        Status = to;
        UpdatedAt = DateTime.UtcNow;

        return ServiceOrderStatusHistory.ForStatusChange(Id, from, to, employeeId);
    }
}
