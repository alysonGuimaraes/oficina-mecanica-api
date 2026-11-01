using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.CommonTestUtilities.Entities;

public static class ServiceOrderBuilder
{
    public static ServiceOrder InStatus(ServiceOrderStatus status, Guid employeeId)
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        if (status == ServiceOrderStatus.RECEIVED)
        {
            return order;
        }

        order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, employeeId);

        switch (status)
        {
            case ServiceOrderStatus.IN_DIAGNOSIS:
                break;
            case ServiceOrderStatus.AWAITING_APPROVAL:
                order.SendBudget(100m, employeeId);
                break;
            case ServiceOrderStatus.AWAITING_MATERIALS:
                order.SendBudget(100m, employeeId);
                order.ChangeStatus(ServiceOrderStatus.AWAITING_MATERIALS, employeeId);
                break;
            case ServiceOrderStatus.IN_EXECUTION:
                order.SendBudget(100m, employeeId);
                order.ChangeStatus(ServiceOrderStatus.AWAITING_MATERIALS, employeeId);
                order.ChangeStatus(ServiceOrderStatus.IN_EXECUTION, employeeId);
                break;
            case ServiceOrderStatus.FINISHED:
                order.Complete("observacao", employeeId);
                break;
            case ServiceOrderStatus.CANCELLED:
                order.Cancel("motivo", employeeId);
                break;
            case ServiceOrderStatus.DELIVERED:
                order.Complete("observacao", employeeId);
                order.ChangeStatus(ServiceOrderStatus.DELIVERED, employeeId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        return order;
    }
}
