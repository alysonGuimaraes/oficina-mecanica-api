using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Employees;

namespace AutoGarage.CommonTestUtilities.Repositories.Employees;

/// <summary>
/// Repositório em memória. Replica a regra de e-mail case-insensitive do repositório real.
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

    public Task Add(Employee employee, CancellationToken cancellationToken)
    {
        _employees.Add(employee);

        return Task.CompletedTask;
    }
}
