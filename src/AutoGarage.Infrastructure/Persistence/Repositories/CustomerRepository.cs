using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerReadOnlyRepository, ICustomerWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByEmail(string email, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AnyAsync(c => c.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsByDocument(string document, CancellationToken cancellationToken)
    {
        var documentValue = new Document(document);

        return await _context.Customers
            .AnyAsync(c => c.Document == documentValue, cancellationToken);
    }

    public async Task<bool> ExistsByEmailExcludingId(string email, Guid id, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AnyAsync(c => c.Id != id && c.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetByStatus(EntityStatus status, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(c => c.Status == status)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByDocument(string document, CancellationToken cancellationToken)
    {
        var documentValue = new Document(document);

        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Document == documentValue, cancellationToken);
    }

    public async Task Add(Customer customer, CancellationToken cancellationToken)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public Task Update(Customer customer, CancellationToken cancellationToken)
    {
        _context.Customers.Update(customer);

        return Task.CompletedTask;
    }
}
