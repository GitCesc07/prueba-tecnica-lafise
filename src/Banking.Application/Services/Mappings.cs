using Banking.Application.Dtos;
using Banking.Domain.Entities;

namespace Banking.Application.Services;

internal static class Mappings
{    
    private static DateTimeOffset Utc(DateTime d) => new(DateTime.SpecifyKind(d, DateTimeKind.Utc));

    public static CustomerResponse ToResponse(this Customer c) =>
        new(c.Id, c.FullName, c.DateOfBirth, c.Gender, c.MonthlyIncome, Utc(c.CreatedAtUtc));

    public static AccountResponse ToResponse(this BankAccount a) =>
        new(a.Id, a.AccountNumber, a.CustomerId, a.Balance, Utc(a.CreatedAtUtc));

    public static TransactionResponse ToResponse(this AccountTransaction t) =>
        new(t.Id, t.Type, t.Amount, Utc(t.TimestampUtc), t.BalanceAfter);
}
