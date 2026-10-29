using AutoGarage.Communication.Responses.Customers;

namespace AutoGarage.Application.UseCases.Customers.ListCustomers;

public interface IListCustomersUseCase
{
    Task<IReadOnlyList<CustomerResponse>> Handle(string? status, CancellationToken cancellationToken);
}
