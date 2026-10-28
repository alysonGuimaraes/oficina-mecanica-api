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

    public async Task<IReadOnlyList<Employee>> GetAll(CancellationToken cancellationToken)
    {
        return await _context.Employees
            .AsNoTracking()
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(Employee employee, CancellationToken cancellationToken)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
    }
}
