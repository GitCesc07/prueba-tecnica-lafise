using Banking.Application.Abstractions;
using Banking.Domain.Entities;

namespace Banking.Tests.Support;

internal sealed class FakeTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan span) => _now += span;
}

internal sealed class NoOpUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}

internal sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<Guid, Customer> _items = new();
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(id));
    public Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        _items[customer.Id] = customer;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryBankAccountRepository : IBankAccountRepository
{
    private readonly Dictionary<string, BankAccount> _items = new();

    public Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(accountNumber));

    public Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken ct = default)
        => Task.FromResult(_items.ContainsKey(accountNumber));

    public Task AddAsync(BankAccount account, CancellationToken ct = default)
    {
        _items[account.AccountNumber] = account;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AccountTransaction>> GetTransactionsAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = _items.Values.First(a => a.Id == accountId);
        IReadOnlyList<AccountTransaction> list = account.Transactions.OrderBy(t => t.TimestampUtc).ToList();
        return Task.FromResult(list);
    }
}

internal sealed class SequenceNumberGenerator(params string[] numbers) : IAccountNumberGenerator
{
    private readonly Queue<string> _queue = new(numbers);
    public string Generate() => _queue.Dequeue();
}
