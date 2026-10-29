using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Domain.Repositories.Customers;

public interface ICustomerReadOnlyRepository
{
    Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken);

    Task<bool> ExistsByDocument(string document, CancellationToken cancellationToken);

    Task<IReadOnlyList<Customer>> GetByStatus(EntityStatus status, CancellationToken cancellationToken);

    Task<Customer?> GetById(Guid id, CancellationToken cancellationToken);
}
