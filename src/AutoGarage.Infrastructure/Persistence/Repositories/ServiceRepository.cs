using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Services;
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

    public async Task Add(Service service, CancellationToken cancellationToken)
    {
        await _context.Services.AddAsync(service, cancellationToken);
    }
}
