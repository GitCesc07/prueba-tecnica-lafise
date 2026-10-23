using System.Globalization;
using System.Security.Cryptography;
using Banking.Application.Abstractions;

namespace Banking.Application.Services;

public sealed class AccountNumberGenerator(TimeProvider clock) : IAccountNumberGenerator
{
    public const string Prefix = "ACC-";

    public string Generate()
    {
        var date = clock.GetUtcNow().ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var suffix = RandomNumberGenerator.GetInt32(0, 10_000);
        return $"{Prefix}{date}-{suffix:D4}";
    }
}
