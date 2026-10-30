using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.Materials;

public interface IMaterialTransactionWriteOnlyRepository
{
    Task Add(MaterialTransaction materialTransaction, CancellationToken cancellationToken);
}
