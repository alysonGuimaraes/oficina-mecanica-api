using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.Employees;

public interface IEmployeeWriteOnlyRepository
{
    Task Add(Employee employee, CancellationToken cancellationToken);

    Task Update(Employee employee, CancellationToken cancellationToken);
}
