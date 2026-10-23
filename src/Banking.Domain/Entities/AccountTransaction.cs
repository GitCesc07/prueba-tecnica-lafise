using Banking.Domain.Enums;

namespace Banking.Domain.Entities;

public sealed class AccountTransaction
{
    private AccountTransaction() { }

    internal AccountTransaction(Guid accountId, TransactionType type, decimal amount,
                                decimal balanceAfter, DateTime timestampUtc)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        TimestampUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public DateTime TimestampUtc { get; private set; }
}
