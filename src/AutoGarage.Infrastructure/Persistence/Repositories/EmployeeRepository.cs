using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Employees;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : IEmployeeReadOnlyRepository, IEmployeeWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken)
    {
        return await _context.Employees
            .AnyAsync(e => e.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsByEmailExcludingId(string email, Guid id, CancellationToken cancellationToken)
    {
        return await _context.Employees
            .AnyAsync(e => e.Id != id && e.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetAll(CancellationToken cancellationToken)
    {
        return await _context.Employees
            .AsNoTracking()
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task Add(Employee employee, CancellationToken cancellationToken)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
    }

    public Task Update(Employee employee, CancellationToken cancellationToken)
    {
        _context.Employees.Update(employee);

        return Task.CompletedTask;
    }
}
