using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;

namespace AutoGarage.Application.UseCases.ServiceOrders.StartServiceOrderExecution;

public interface IStartServiceOrderExecutionUseCase
{
    Task<ServiceOrderResponse> Handle(ServiceOrderEmployeeCommand command, CancellationToken cancellationToken);
}
