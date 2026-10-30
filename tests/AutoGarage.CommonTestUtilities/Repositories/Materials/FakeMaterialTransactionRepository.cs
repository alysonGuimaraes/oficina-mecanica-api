using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Materials;
using AutoGarage.Domain.Repositories.Materials;

namespace AutoGarage.CommonTestUtilities.Repositories.Materials;

public class FakeMaterialTransactionRepository : IMaterialTransactionWriteOnlyRepository, IMaterialTransactionReadOnlyRepository
{
    private readonly List<MaterialTransaction> _transactions = [];

    public IReadOnlyList<MaterialTransaction> Transactions => _transactions;

    public Task Add(MaterialTransaction materialTransaction, CancellationToken cancellationToken)
    {
        _transactions.Add(materialTransaction);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MaterialTransaction>> GetByMaterialId(Guid materialId, TransactionType? type, CancellationToken cancellationToken)
    {
        IReadOnlyList<MaterialTransaction> ordered = _transactions
            .Where(t => t.MaterialId == materialId && (type is null || t.Type == type))
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .ToList();

        return Task.FromResult(ordered);
    }
}
