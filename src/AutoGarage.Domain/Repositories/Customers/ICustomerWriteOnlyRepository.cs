using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.Customers;

public interface ICustomerWriteOnlyRepository
{
    Task Add(Customer customer, CancellationToken cancellationToken);
}
