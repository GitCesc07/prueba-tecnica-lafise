namespace Banking.Domain.Exceptions;

public abstract class BusinessException : Exception
{
    protected BusinessException(string code, string message) : base(message) => Code = code;
    public string Code { get; }
}

public abstract class NotFoundException(string code, string message) : BusinessException(code, message);

public sealed class AccountNotFoundException(string accountNumber)
    : NotFoundException("account_not_found", $"La cuenta '{accountNumber}' no existe.");

public sealed class CustomerNotFoundException(Guid customerId)
    : NotFoundException("customer_not_found", $"El cliente '{customerId}' no existe.");

public sealed class InsufficientFundsException(decimal balance, decimal requested)
    : BusinessException("insufficient_funds",
        $"Fondos insuficientes. Saldo disponible: {balance:0.00}, monto solicitado: {requested:0.00}.");

public sealed class DomainValidationException(string message)
    : BusinessException("validation_error", message);
