namespace AutoGarage.Domain.Repositories.Materials;

public interface IMaterialReadOnlyRepository
{
    Task<bool> ExistsByName(string name, CancellationToken cancellationToken);
}
