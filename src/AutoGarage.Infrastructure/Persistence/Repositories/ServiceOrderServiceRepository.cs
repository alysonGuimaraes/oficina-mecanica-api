using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class ServiceOrderServiceRepository : IServiceOrderServiceReadOnlyRepository, IServiceOrderServiceWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public ServiceOrderServiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceOrderService?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrderServices
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    // Devolve a linha ativa ou inativa. Quem chama decide o que fazer.
    public async Task<ServiceOrderService?> GetByServiceOrderAndServiceId(Guid serviceOrderId, Guid serviceId, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrderServices
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServiceOrderId == serviceOrderId && s.ServiceId == serviceId, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceOrderService>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrderServices
            .AsNoTracking()
            .Where(s => s.ServiceOrderId == serviceOrderId)
            .OrderBy(s => s.CreatedAt)
            .ThenBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(ServiceOrderService item, CancellationToken cancellationToken)
    {
        await _context.ServiceOrderServices.AddAsync(item, cancellationToken);
    }

    public Task Update(ServiceOrderService item, CancellationToken cancellationToken)
    {
        _context.ServiceOrderServices.Update(item);

        return Task.CompletedTask;
    }
}
