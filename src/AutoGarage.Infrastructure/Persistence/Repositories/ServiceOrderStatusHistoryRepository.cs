using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class ServiceOrderStatusHistoryRepository : IServiceOrderStatusHistoryReadOnlyRepository, IServiceOrderStatusHistoryWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public ServiceOrderStatusHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ServiceOrderStatusHistory>> GetAllByServiceOrder(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrderStatusHistories
            .AsNoTracking()
            .Where(h => h.ServiceOrderId == serviceOrderId)
            .OrderBy(h => h.CreatedAt)
            .ThenBy(h => h.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(ServiceOrderStatusHistory history, CancellationToken cancellationToken)
    {
        await _context.ServiceOrderStatusHistories.AddAsync(history, cancellationToken);
    }
}
