using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositories;

internal sealed class CustomerRepository(BankingDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
        => await db.Customers.AddAsync(customer, ct);
}
