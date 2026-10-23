using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("Customers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        b.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.MonthlyIncome).HasPrecision(18, 2);
    }
}

internal sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> b)
    {
        b.ToTable("Accounts");
        b.HasKey(x => x.Id);        
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.AccountNumber).IsRequired().HasMaxLength(17);
        b.HasIndex(x => x.AccountNumber).IsUnique();

        b.Property(x => x.Balance).HasPrecision(18, 2);

        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Transactions).WithOne().HasForeignKey(t => t.AccountId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class AccountTransactionConfiguration : IEntityTypeConfiguration<AccountTransaction>
{
    public void Configure(EntityTypeBuilder<AccountTransaction> b)
    {
        b.ToTable("Transactions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        b.HasIndex(x => new { x.AccountId, x.TimestampUtc });
    }
}
