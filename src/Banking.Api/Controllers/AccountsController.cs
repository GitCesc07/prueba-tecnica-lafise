using Banking.Application.Dtos;
using Banking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(IAccountService accounts) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AccountResponse>> Create(CreateAccountRequest request, CancellationToken ct)
    {
        var created = await accounts.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetBalance), new { accountNumber = created.AccountNumber }, created);
    }

    [HttpGet("{accountNumber}/balance")]
    public async Task<ActionResult<BalanceResponse>> GetBalance(string accountNumber, CancellationToken ct)
        => Ok(await accounts.GetBalanceAsync(accountNumber, ct));

    [HttpPost("{accountNumber}/deposits")]
    public async Task<ActionResult<TransactionResponse>> Deposit(string accountNumber, TransactionRequest request, CancellationToken ct)
        => Ok(await accounts.DepositAsync(accountNumber, request.Amount, ct));

    [HttpPost("{accountNumber}/withdrawals")]
    public async Task<ActionResult<TransactionResponse>> Withdraw(string accountNumber, TransactionRequest request, CancellationToken ct)
        => Ok(await accounts.WithdrawAsync(accountNumber, request.Amount, ct));

    [HttpGet("{accountNumber}/transactions")]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> GetHistory(string accountNumber, CancellationToken ct)
        => Ok(await accounts.GetHistoryAsync(accountNumber, ct));
}
