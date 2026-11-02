using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Application.Services.Notifications;

// Business labels used in customer-facing text. They follow the ubiquitous language glossary.
public static class ServiceOrderStatusLabels
{
    public static string For(ServiceOrderStatus status) => status switch
    {
        ServiceOrderStatus.RECEIVED => "Recebida",
        ServiceOrderStatus.IN_DIAGNOSIS => "Diagnóstico",
        ServiceOrderStatus.AWAITING_APPROVAL => "Aguardando Aprovação",
        ServiceOrderStatus.AWAITING_MATERIALS => "Aguardando Materiais",
        ServiceOrderStatus.IN_EXECUTION => "Em Execução",
        ServiceOrderStatus.FINISHED => "Finalizada",
        ServiceOrderStatus.CANCELLED => "Cancelada",
        ServiceOrderStatus.DELIVERED => "Entregue",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown service order status.")
    };
}
