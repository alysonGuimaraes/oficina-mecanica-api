using AutoGarage.Application.Commands.Customers;
using AutoGarage.Communication.Responses.Customers;

namespace AutoGarage.Application.UseCases.Customers.UpdateCustomerStatus;

public interface IUpdateCustomerStatusUseCase
{
    Task<CustomerResponse> Handle(UpdateCustomerStatusCommand command, CancellationToken cancellationToken);
}
