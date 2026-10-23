using Banking.Application.Abstractions;
using Banking.Application.Dtos;
using Banking.Application.Services;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using Banking.Tests.Support;

namespace Banking.Tests;

public class AccountServiceTests
{
    private sealed class Fixture
    {
        public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero));
        public InMemoryBankAccountRepository Accounts { get; } = new();
        public InMemoryCustomerRepository Customers { get; } = new();
        public AccountService Service { get; }

        public Fixture(IAccountNumberGenerator? generator = null)
        {
            Service = new AccountService(Accounts, Customers,
                generator ?? new AccountNumberGenerator(Clock), new NoOpUnitOfWork(), Clock);
        }

        public async Task<AccountResponse> SeedAccountAsync(decimal initialBalance)
        {
            var customers = new CustomerService(Customers, new NoOpUnitOfWork(), Clock);
            var customer = await customers.CreateAsync(
                new CreateCustomerRequest("César Urroz", new DateOnly(1997, 07, 04), Gender.Male, 2500m));
            return await Service.CreateAsync(new CreateAccountRequest(customer.Id, initialBalance));
        }
    }

    [Fact]
    public async Task CreateAccount_WithUnknownCustomer_ThrowsCustomerNotFound()
    {
        var f = new Fixture();
        await Assert.ThrowsAsync<CustomerNotFoundException>(
            () => f.Service.CreateAsync(new CreateAccountRequest(Guid.NewGuid(), 10m)));
    }

    [Fact]
    public async Task CreateAccount_GeneratesNumberWithTodaysDate_AndInitialBalance()
    {
        var f = new Fixture();
        var account = await f.SeedAccountAsync(250m);

        Assert.StartsWith("ACC-20261022-", account.AccountNumber);
        Assert.Equal(250m, account.Balance);
    }

    [Fact]
    public async Task CreateAccount_RetriesWhenGeneratedNumberAlreadyExists()
    {
        var generator = new SequenceNumberGenerator("ACC-20261022-0001", "ACC-20261022-0001", "ACC-20261022-0002");
        var f = new Fixture(generator);

        var first = await f.SeedAccountAsync(0m);
        var second = await f.SeedAccountAsync(0m);

        Assert.Equal("ACC-20261022-0001", first.AccountNumber);
        Assert.Equal("ACC-20261022-0002", second.AccountNumber);
    }

    [Fact]
    public async Task Deposit_IncreasesBalance()
    {
        var f = new Fixture();
        var account = await f.SeedAccountAsync(100m);
        var tx = await f.Service.DepositAsync(account.AccountNumber, 50m);
        var balance = await f.Service.GetBalanceAsync(account.AccountNumber);
        Assert.Equal(150m, tx.BalanceAfter);
        Assert.Equal(150m, balance.Balance);
    }

    [Fact]
    public async Task Withdraw_WithFunds_DecreasesBalance()
    {
        var f = new Fixture();
        var account = await f.SeedAccountAsync(100m);
        await f.Service.WithdrawAsync(account.AccountNumber, 30m);
        Assert.Equal(70m, (await f.Service.GetBalanceAsync(account.AccountNumber)).Balance);
    }

    [Fact]
    public async Task Withdraw_WithoutFunds_ThrowsAndKeepsBalanceAndHistory()
    {
        var f = new Fixture();
        var account = await f.SeedAccountAsync(100m);
        await Assert.ThrowsAsync<InsufficientFundsException>(
            () => f.Service.WithdrawAsync(account.AccountNumber, 500m));
        Assert.Equal(100m, (await f.Service.GetBalanceAsync(account.AccountNumber)).Balance);
        Assert.Single(await f.Service.GetHistoryAsync(account.AccountNumber));
    }

    [Fact]
    public async Task GetBalance_UnknownAccount_ThrowsAccountNotFound()
    {
        var f = new Fixture();
        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => f.Service.GetBalanceAsync("ACC-20261022-9999"));
    }

    [Fact]
    public async Task History_IsChronological_WithHistoricalBalances()
    {
        var f = new Fixture();
        var account = await f.SeedAccountAsync(100m);
        f.Clock.Advance(TimeSpan.FromMinutes(1));
        await f.Service.DepositAsync(account.AccountNumber, 50m);
        f.Clock.Advance(TimeSpan.FromMinutes(1));
        await f.Service.WithdrawAsync(account.AccountNumber, 70m);

        var history = await f.Service.GetHistoryAsync(account.AccountNumber);

        Assert.Equal(3, history.Count);
        Assert.Equal(new[] { 100m, 150m, 80m }, history.Select(h => h.BalanceAfter));
        Assert.Equal(new[] { TransactionType.Deposit, TransactionType.Deposit, TransactionType.Withdrawal },
                     history.Select(h => h.Type));
        Assert.Equal(history.OrderBy(h => h.Timestamp).ToList(), history);
        Assert.Equal(3, history.Select(h => h.TransactionId).Distinct().Count());
    }
}
