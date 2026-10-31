using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class ServiceOrderRepository : IServiceOrderReadOnlyRepository, IServiceOrderWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public ServiceOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceOrder?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceOrder>> GetAll(ServiceOrderStatus? status, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => status == null || o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(ServiceOrder serviceOrder, CancellationToken cancellationToken)
    {
        await _context.ServiceOrders.AddAsync(serviceOrder, cancellationToken);
    }

    public Task Update(ServiceOrder serviceOrder, CancellationToken cancellationToken)
    {
        _context.ServiceOrders.Update(serviceOrder);

        return Task.CompletedTask;
    }
}
