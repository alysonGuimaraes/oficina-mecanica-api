using AutoGarage.Application.Services.Notifications;
using AutoGarage.Domain.Enums.ServiceOrders;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.Notifications;

public class ServiceOrderStatusLabelsTests
{
    [Theory]
    [InlineData(ServiceOrderStatus.RECEIVED, "Recebida")]
    [InlineData(ServiceOrderStatus.IN_DIAGNOSIS, "Diagnóstico")]
    [InlineData(ServiceOrderStatus.AWAITING_APPROVAL, "Aguardando Aprovação")]
    [InlineData(ServiceOrderStatus.AWAITING_MATERIALS, "Aguardando Materiais")]
    [InlineData(ServiceOrderStatus.IN_EXECUTION, "Em Execução")]
    [InlineData(ServiceOrderStatus.FINISHED, "Finalizada")]
    [InlineData(ServiceOrderStatus.CANCELLED, "Cancelada")]
    [InlineData(ServiceOrderStatus.DELIVERED, "Entregue")]
    public void For_EveryStatus_ShouldReturnBusinessLabel(ServiceOrderStatus status, string expected)
    {
        ServiceOrderStatusLabels.For(status).ShouldBe(expected);
    }
}
