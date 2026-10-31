using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class ServiceOrderMaterialRepository : IServiceOrderMaterialReadOnlyRepository, IServiceOrderMaterialWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public ServiceOrderMaterialRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceOrderMaterial?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrderMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    // Devolve a linha ativa ou inativa. Quem chama decide o que fazer.
    public async Task<ServiceOrderMaterial?> GetByServiceOrderAndMaterialId(Guid serviceOrderId, Guid materialId, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrderMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ServiceOrderId == serviceOrderId && m.MaterialId == materialId, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceOrderMaterial>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrderMaterials
            .AsNoTracking()
            .Where(m => m.ServiceOrderId == serviceOrderId)
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(ServiceOrderMaterial item, CancellationToken cancellationToken)
    {
        await _context.ServiceOrderMaterials.AddAsync(item, cancellationToken);
    }

    public Task Update(ServiceOrderMaterial item, CancellationToken cancellationToken)
    {
        _context.ServiceOrderMaterials.Update(item);

        return Task.CompletedTask;
    }
}
