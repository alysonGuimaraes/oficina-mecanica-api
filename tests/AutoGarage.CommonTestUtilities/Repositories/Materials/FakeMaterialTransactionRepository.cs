using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Repositories.MaterialTransactions;

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

    public Task<IReadOnlyList<MaterialTransaction>> GetAll(TransactionType? type, CancellationToken cancellationToken)
    {
        IReadOnlyList<MaterialTransaction> ordered = _transactions
            .Where(t => type is null || t.Type == type)
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task<MaterialTransaction?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_transactions.FirstOrDefault(t => t.Id == id));
    }
}
