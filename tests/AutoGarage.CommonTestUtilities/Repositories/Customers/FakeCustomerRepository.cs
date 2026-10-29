using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Shared;

namespace AutoGarage.CommonTestUtilities.Repositories.Customers;

/// <summary>
/// Repositório em memória. E-mail sem diferenciar caixa; documento pelo valor exato, como no repositório real.
/// </summary>
public class FakeCustomerRepository : ICustomerReadOnlyRepository, ICustomerWriteOnlyRepository
{
    private readonly List<Customer> _customers = [];

    public IReadOnlyList<Customer> Customers => _customers;

    public FakeCustomerRepository WithCustomer(Customer customer)
    {
        _customers.Add(customer);
        return this;
    }

    public Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken)
    {
        var exists = _customers.Any(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }

    public Task<bool> ExistsByDocument(string document, CancellationToken cancellationToken)
    {
        var exists = _customers.Any(c => c.Document.Value == document);

        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Customer>> GetByStatus(EntityStatus status, CancellationToken cancellationToken)
    {
        IReadOnlyList<Customer> ordered = _customers
            .Where(c => c.Status == status)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task<Customer?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task<Customer?> GetByDocument(string document, CancellationToken cancellationToken)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Document.Value == document));
    }

    public Task Add(Customer customer, CancellationToken cancellationToken)
    {
        _customers.Add(customer);

        return Task.CompletedTask;
    }
}
