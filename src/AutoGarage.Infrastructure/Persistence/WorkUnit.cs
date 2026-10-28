using AutoGarage.Domain.Repositories;

namespace AutoGarage.Infrastructure.Persistence;

public class WorkUnit : IUnitWork
{
    private readonly AppDbContext _dbContext;

    public WorkUnit(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task Commit() => await _dbContext.SaveChangesAsync();
}
