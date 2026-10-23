using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Tests;

public class BankAccountTests
{
    private static readonly DateTime Now = new(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);
    private static BankAccount NewAccount(decimal initial = 100m)
        => BankAccount.Open(Guid.NewGuid(), "ACC-20261022-0001", initial, Now);

    [Fact]
    public void Open_WithInitialBalance_RegistersInitialDeposit()
    {
        var account = NewAccount(100m);

        var tx = Assert.Single(account.Transactions);
        Assert.Equal(TransactionType.Deposit, tx.Type);
        Assert.Equal(100m, tx.BalanceAfter);
    }

    [Fact]
    public void Deposit_IncreasesBalance()
    {
        var account = NewAccount(100m);

        var tx = account.Deposit(50m, Now);

        Assert.Equal(150m, account.Balance);
        Assert.Equal(150m, tx.BalanceAfter);
    }

    [Fact]
    public void Withdraw_WithSufficientFunds_DecreasesBalance()
    {
        var account = NewAccount(100m);

        var tx = account.Withdraw(40m, Now);

        Assert.Equal(60m, account.Balance);
        Assert.Equal(TransactionType.Withdrawal, tx.Type);
        Assert.Equal(60m, tx.BalanceAfter);
    }

    [Fact]
    public void Withdraw_ExactBalance_IsAllowed()
    {
        var account = NewAccount(100m);
        account.Withdraw(100m, Now);
        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public void Withdraw_WithInsufficientFunds_Throws_AndDoesNotChangeState()
    {
        var account = NewAccount(100m);
        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(100.01m, Now));
        Assert.Equal(100m, account.Balance);
        Assert.Single(account.Transactions);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(10.123)]
    public void InvalidAmounts_AreRejected(double raw)
    {
        var account = NewAccount();
        var amount = (decimal)raw;

        Assert.Throws<DomainValidationException>(() => account.Deposit(amount, Now));
        Assert.Throws<DomainValidationException>(() => account.Withdraw(amount, Now));
    }

    [Fact]
    public void Open_WithNegativeInitialBalance_Throws()
        => Assert.Throws<DomainValidationException>(() => NewAccount(-1m));
}
