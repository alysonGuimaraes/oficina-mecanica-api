using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Customers;
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

    public async Task Add(Customer customer, CancellationToken cancellationToken)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }
}
