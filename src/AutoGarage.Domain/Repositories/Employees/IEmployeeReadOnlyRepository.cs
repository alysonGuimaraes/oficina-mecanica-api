using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.Employees;

public interface IEmployeeReadOnlyRepository
{
    Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken);

    Task<IReadOnlyList<Employee>> GetAll(CancellationToken cancellationToken);

    Task<Employee?> GetById(Guid id, CancellationToken cancellationToken);
}
