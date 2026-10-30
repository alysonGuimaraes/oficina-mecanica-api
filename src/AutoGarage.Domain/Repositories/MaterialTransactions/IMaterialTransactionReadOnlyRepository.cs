using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;

namespace AutoGarage.Domain.Repositories.MaterialTransactions;

public interface IMaterialTransactionReadOnlyRepository
{
    Task<IReadOnlyList<MaterialTransaction>> GetByMaterialId(Guid materialId, TransactionType? type, CancellationToken cancellationToken);

    Task<IReadOnlyList<MaterialTransaction>> GetAll(TransactionType? type, CancellationToken cancellationToken);

    Task<MaterialTransaction?> GetById(Guid id, CancellationToken cancellationToken);
}
