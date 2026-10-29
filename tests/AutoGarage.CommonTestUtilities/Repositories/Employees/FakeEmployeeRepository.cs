using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Employees;

namespace AutoGarage.CommonTestUtilities.Repositories.Employees;

/// <summary>
/// Repositório em memória. Replica a regra de e-mail case-insensitive e a ordenação do repositório real.
/// </summary>
public class FakeEmployeeRepository : IEmployeeReadOnlyRepository, IEmployeeWriteOnlyRepository
{
    private readonly List<Employee> _employees = [];

    public IReadOnlyList<Employee> Employees => _employees;

    public FakeEmployeeRepository WithEmployee(Employee employee)
    {
        _employees.Add(employee);
        return this;
    }

    public Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken)
    {
        var exists = _employees.Any(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }

    public Task<bool> ExistsByEmailExcludingId(string email, Guid id, CancellationToken cancellationToken)
    {
        var exists = _employees.Any(e => e.Id != id && e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Employee>> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<Employee> ordered = _employees
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task<Employee?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));
    }

    public Task Add(Employee employee, CancellationToken cancellationToken)
    {
        _employees.Add(employee);

        return Task.CompletedTask;
    }

    public Task Update(Employee employee, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
