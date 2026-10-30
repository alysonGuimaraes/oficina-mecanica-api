using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Materials;

namespace AutoGarage.Domain.Repositories.Materials;

public interface IMaterialTransactionReadOnlyRepository
{
    Task<IReadOnlyList<MaterialTransaction>> GetByMaterialId(Guid materialId, TransactionType? type, CancellationToken cancellationToken);
}
