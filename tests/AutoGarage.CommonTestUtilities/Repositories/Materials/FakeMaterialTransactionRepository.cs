using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Materials;

namespace AutoGarage.CommonTestUtilities.Repositories.Materials;

public class FakeMaterialTransactionRepository : IMaterialTransactionWriteOnlyRepository
{
    private readonly List<MaterialTransaction> _transactions = [];

    public IReadOnlyList<MaterialTransaction> Transactions => _transactions;

    public Task Add(MaterialTransaction materialTransaction, CancellationToken cancellationToken)
    {
        _transactions.Add(materialTransaction);

        return Task.CompletedTask;
    }
}
