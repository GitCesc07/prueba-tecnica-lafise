using Banking.Application.Abstractions;
using Banking.Application.Dtos;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;

namespace Banking.Application.Services;

public sealed class AccountService(
    IBankAccountRepository accounts,
    ICustomerRepository customers,
    IAccountNumberGenerator numberGenerator,
    IUnitOfWork uow,
    TimeProvider clock) : IAccountService
{
    private const int MaxNumberGenerationAttempts = 10;

    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        _ = await customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        var number = await GenerateUniqueNumberAsync(ct);
        var account = BankAccount.Open(request.CustomerId, number, request.InitialBalance, Now());

        await accounts.AddAsync(account, ct);
        await uow.SaveChangesAsync(ct);
        return account.ToResponse();
    }

    public async Task<BalanceResponse> GetBalanceAsync(string accountNumber, CancellationToken ct = default)
    {
        var account = await GetAccountAsync(accountNumber, ct);
        return new BalanceResponse(account.AccountNumber, account.Balance);
    }

    public async Task<TransactionResponse> DepositAsync(string accountNumber, decimal amount, CancellationToken ct = default)
    {
        var account = await GetAccountAsync(accountNumber, ct);
        var tx = account.Deposit(amount, Now());
        await uow.SaveChangesAsync(ct);
        return tx.ToResponse();
    }

    public async Task<TransactionResponse> WithdrawAsync(string accountNumber, decimal amount, CancellationToken ct = default)
    {
        var account = await GetAccountAsync(accountNumber, ct);
        var tx = account.Withdraw(amount, Now());
        await uow.SaveChangesAsync(ct);
        return tx.ToResponse();
    }

    public async Task<IReadOnlyList<TransactionResponse>> GetHistoryAsync(string accountNumber, CancellationToken ct = default)
    {
        var account = await GetAccountAsync(accountNumber, ct);
        var txs = await accounts.GetTransactionsAsync(account.Id, ct);
        return txs.Select(t => t.ToResponse()).ToList();
    }

    private async Task<BankAccount> GetAccountAsync(string accountNumber, CancellationToken ct)
    {
        var normalized = (accountNumber ?? string.Empty).Trim().ToUpperInvariant();
        return await accounts.GetByAccountNumberAsync(normalized, ct)
               ?? throw new AccountNotFoundException(normalized);
    }

    private async Task<string> GenerateUniqueNumberAsync(CancellationToken ct)
    {        
        for (var i = 0; i < MaxNumberGenerationAttempts; i++)
        {
            var candidate = numberGenerator.Generate();
            if (!await accounts.AccountNumberExistsAsync(candidate, ct))
                return candidate;
        }
        throw new InvalidOperationException("No fue posible generar un número de cuenta único.");
    }

    private DateTime Now() => clock.GetUtcNow().UtcDateTime;
}
