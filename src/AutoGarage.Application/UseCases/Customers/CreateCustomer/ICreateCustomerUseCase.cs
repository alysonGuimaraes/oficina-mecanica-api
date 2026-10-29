using AutoGarage.Application.Commands.Customers;
using AutoGarage.Communication.Responses.Customers;

namespace AutoGarage.Application.UseCases.Customers.CreateCustomer;

public interface ICreateCustomerUseCase
{
    Task<CustomerResponse> Handle(CreateCustomerCommand command, CancellationToken cancellationToken);
}
