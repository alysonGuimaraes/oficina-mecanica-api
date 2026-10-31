using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.SendServiceOrderBudget;

public interface ISendServiceOrderBudgetUseCase
{
    Task<ServiceOrderResponse> Handle(ServiceOrderEmployeeCommand command, CancellationToken cancellationToken);
}
