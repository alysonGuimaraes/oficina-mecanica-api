using AutoGarage.Communication.Responses.Customers;

namespace AutoGarage.Application.UseCases.Customers.GetCustomerByDocument;

public interface IGetCustomerByDocumentUseCase
{
    Task<CustomerResponse> Handle(string? document, CancellationToken cancellationToken);
}
