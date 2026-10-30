namespace AutoGarage.Domain.Repositories.Services;

public interface IServiceReadOnlyRepository
{
    Task<bool> ExistsByName(string name, CancellationToken cancellationToken);
}
