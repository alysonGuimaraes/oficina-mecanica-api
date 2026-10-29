using AutoGarage.Application.Commands.Customers;
using AutoGarage.Communication.Responses.Customers;

namespace AutoGarage.Application.UseCases.Customers.UpdateCustomer;

public interface IUpdateCustomerUseCase
{
    Task<CustomerResponse> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken);
}
