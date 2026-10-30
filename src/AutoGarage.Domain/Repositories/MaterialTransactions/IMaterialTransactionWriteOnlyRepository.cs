using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.MaterialTransactions;

public interface IMaterialTransactionWriteOnlyRepository
{
    Task Add(MaterialTransaction materialTransaction, CancellationToken cancellationToken);
}
