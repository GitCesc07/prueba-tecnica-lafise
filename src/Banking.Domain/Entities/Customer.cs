using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Domain.Entities;

public sealed class Customer
{
    private Customer() { }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public decimal MonthlyIncome { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Customer Create(string fullName, DateOnly dateOfBirth, Gender gender,
                                  decimal monthlyIncome, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainValidationException("El nombre completo es obligatorio.");
        if (fullName.Trim().Length > 200)
            throw new DomainValidationException("El nombre completo no puede exceder 200 caracteres.");
        if (dateOfBirth > DateOnly.FromDateTime(nowUtc))
            throw new DomainValidationException("La fecha de nacimiento no puede ser futura.");
        if (!Enum.IsDefined(gender))
            throw new DomainValidationException("El sexo/género indicado no es válido.");
        if (monthlyIncome < 0)
            throw new DomainValidationException("Los ingresos mensuales no pueden ser negativos.");

        return new Customer
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,
            MonthlyIncome = monthlyIncome,
            CreatedAtUtc = nowUtc
        };
    }
}
