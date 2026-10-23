using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Domain.Entities;

public sealed class BankAccount
{
    private readonly List<AccountTransaction> _transactions = new();

    private BankAccount() { }

    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<AccountTransaction> Transactions => _transactions;

    public static BankAccount Open(Guid customerId, string accountNumber, decimal initialBalance, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new DomainValidationException("El número de cuenta es obligatorio.");
        if (initialBalance < 0)
            throw new DomainValidationException("El saldo inicial no puede ser negativo.");
        EnsureMaxTwoDecimals(initialBalance);

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            CustomerId = customerId,
            CreatedAtUtc = nowUtc
        };
        
        if (initialBalance > 0)
            account.Deposit(initialBalance, nowUtc);

        return account;
    }

    public AccountTransaction Deposit(decimal amount, DateTime nowUtc)
    {
        ValidateAmount(amount);
        Balance += amount;
        return Register(TransactionType.Deposit, amount, nowUtc);
    }

    public AccountTransaction Withdraw(decimal amount, DateTime nowUtc)
    {
        ValidateAmount(amount);
        if (amount > Balance)
            throw new InsufficientFundsException(Balance, amount);

        Balance -= amount;
        return Register(TransactionType.Withdrawal, amount, nowUtc);
    }

    private AccountTransaction Register(TransactionType type, decimal amount, DateTime nowUtc)
    {
        var tx = new AccountTransaction(Id, type, amount, Balance, nowUtc);
        _transactions.Add(tx);
        return tx;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("El monto debe ser mayor que cero.");
        EnsureMaxTwoDecimals(amount);
    }

    private static void EnsureMaxTwoDecimals(decimal amount)
    {
        if (decimal.Round(amount, 2) != amount)
            throw new DomainValidationException("El monto admite como máximo 2 decimales.");
    }
}
