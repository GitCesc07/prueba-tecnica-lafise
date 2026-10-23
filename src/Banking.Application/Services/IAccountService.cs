using Banking.Application.Dtos;

namespace Banking.Application.Services;

public interface IAccountService
{
    Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct = default);
    Task<BalanceResponse> GetBalanceAsync(string accountNumber, CancellationToken ct = default);
    Task<TransactionResponse> DepositAsync(string accountNumber, decimal amount, CancellationToken ct = default);
    Task<TransactionResponse> WithdrawAsync(string accountNumber, decimal amount, CancellationToken ct = default);
    Task<IReadOnlyList<TransactionResponse>> GetHistoryAsync(string accountNumber, CancellationToken ct = default);
}
