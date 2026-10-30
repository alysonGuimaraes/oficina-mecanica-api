using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Services;
using AutoGarage.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class ServiceRepository : IServiceReadOnlyRepository, IServiceWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public ServiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByName(string name, CancellationToken cancellationToken)
    {
        return await _context.Services
            .AnyAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByStatus(EntityStatus status, CancellationToken cancellationToken)
    {
        return await _context.Services
            .AsNoTracking()
            .Where(s => s.Status == status)
            .OrderBy(s => s.CreatedAt)
            .ThenBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(Service service, CancellationToken cancellationToken)
    {
        await _context.Services.AddAsync(service, cancellationToken);
    }
}
