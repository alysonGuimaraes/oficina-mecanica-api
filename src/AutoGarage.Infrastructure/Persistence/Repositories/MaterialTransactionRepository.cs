using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Materials;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class MaterialTransactionRepository : IMaterialTransactionWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public MaterialTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Add(MaterialTransaction materialTransaction, CancellationToken cancellationToken)
    {
        await _context.MaterialTransactions.AddAsync(materialTransaction, cancellationToken);
    }
}
