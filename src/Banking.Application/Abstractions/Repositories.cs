using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
}

public interface IBankAccountRepository
{
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken ct = default);
    Task AddAsync(BankAccount account, CancellationToken ct = default);
    Task<IReadOnlyList<AccountTransaction>> GetTransactionsAsync(Guid accountId, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
