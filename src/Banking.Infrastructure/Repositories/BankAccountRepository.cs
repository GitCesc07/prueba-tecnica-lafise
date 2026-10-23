using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositories;

internal sealed class BankAccountRepository(BankingDbContext db) : IBankAccountRepository
{
    public Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
        => db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);

    public Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken ct = default)
        => db.Accounts.AnyAsync(a => a.AccountNumber == accountNumber, ct);

    public async Task AddAsync(BankAccount account, CancellationToken ct = default)
        => await db.Accounts.AddAsync(account, ct);

    public async Task<IReadOnlyList<AccountTransaction>> GetTransactionsAsync(Guid accountId, CancellationToken ct = default)
        => await db.Transactions.AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderBy(t => t.TimestampUtc)
            .ToListAsync(ct);
}
