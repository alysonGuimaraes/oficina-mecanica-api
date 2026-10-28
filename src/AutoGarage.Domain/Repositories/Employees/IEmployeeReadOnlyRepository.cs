namespace AutoGarage.Domain.Repositories.Employees;

public interface IEmployeeReadOnlyRepository
{
    Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken);
}

