using AutoGarage.Communication.Responses.Customers;

namespace AutoGarage.Application.UseCases.Customers.GetCustomerById;

public interface IGetCustomerByIdUseCase
{
    Task<CustomerResponse> Handle(string id, CancellationToken cancellationToken);
}
