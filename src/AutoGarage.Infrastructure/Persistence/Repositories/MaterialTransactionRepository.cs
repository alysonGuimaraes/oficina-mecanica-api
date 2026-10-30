using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Materials;
using AutoGarage.Domain.Repositories.Materials;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class MaterialTransactionRepository : IMaterialTransactionWriteOnlyRepository, IMaterialTransactionReadOnlyRepository
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

    public async Task<IReadOnlyList<MaterialTransaction>> GetByMaterialId(Guid materialId, TransactionType? type, CancellationToken cancellationToken)
    {
        return await _context.MaterialTransactions
            .AsNoTracking()
            .Where(t => t.MaterialId == materialId && (type == null || t.Type == type))
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MaterialTransaction>> GetAll(TransactionType? type, CancellationToken cancellationToken)
    {
        return await _context.MaterialTransactions
            .AsNoTracking()
            .Where(t => type == null || t.Type == type)
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaterialTransaction?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _context.MaterialTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}
