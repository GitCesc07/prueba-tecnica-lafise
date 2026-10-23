using Banking.Domain.Enums;

namespace Banking.Application.Dtos;

public sealed record CreateCustomerRequest(string FullName, DateOnly DateOfBirth, Gender Gender, decimal MonthlyIncome);

public sealed record CustomerResponse(Guid Id, string FullName, DateOnly DateOfBirth, Gender Gender,
                                      decimal MonthlyIncome, DateTimeOffset CreatedAt);

public sealed record CreateAccountRequest(Guid CustomerId, decimal InitialBalance);

public sealed record AccountResponse(Guid Id, string AccountNumber, Guid CustomerId,
                                     decimal Balance, DateTimeOffset CreatedAt);

public sealed record BalanceResponse(string AccountNumber, decimal Balance);

public sealed record TransactionRequest(decimal Amount);

public sealed record TransactionResponse(Guid TransactionId, TransactionType Type, decimal Amount,
                                         DateTimeOffset Timestamp, decimal BalanceAfter);
