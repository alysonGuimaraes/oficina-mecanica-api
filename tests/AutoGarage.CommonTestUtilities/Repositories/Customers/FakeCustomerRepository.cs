using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Customers;

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

    public Task Add(Customer customer, CancellationToken cancellationToken)
    {
        _customers.Add(customer);

        return Task.CompletedTask;
    }
}
