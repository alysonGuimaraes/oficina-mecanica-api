namespace AutoGarage.Domain.Repositories.Customers;

public interface ICustomerReadOnlyRepository
{
    Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken);

    Task<bool> ExistsByDocument(string document, CancellationToken cancellationToken);
}
