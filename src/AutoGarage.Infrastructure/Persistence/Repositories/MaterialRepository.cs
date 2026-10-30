using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Materials;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class MaterialRepository : IMaterialReadOnlyRepository, IMaterialWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public MaterialRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByName(string name, CancellationToken cancellationToken)
    {
        return await _context.Materials
            .AnyAsync(m => m.Name == name, cancellationToken);
    }

    public async Task Add(Material material, CancellationToken cancellationToken)
    {
        await _context.Materials.AddAsync(material, cancellationToken);
    }
}
