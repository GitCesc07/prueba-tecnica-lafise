using System.Text.RegularExpressions;
using Banking.Application.Services;
using Banking.Tests.Support;

namespace Banking.Tests;

public class AccountNumberGeneratorTests
{
    [Fact]
    public void Generate_ReturnsExpectedFormat_Prefix_Length_And_Digits()
    {
        var sut = new AccountNumberGenerator(TimeProvider.System);

        var number = sut.Generate();

        Assert.StartsWith("ACC-", number);
        Assert.Equal(17, number.Length);
        Assert.Matches(new Regex(@"^ACC-\d{8}-\d{4}$"), number);
    }

    [Fact]
    public void Generate_UsesTodaysDate()
    {
        var sut = new AccountNumberGenerator(TimeProvider.System);
        var number = sut.Generate();
        Assert.Equal(DateTime.UtcNow.ToString("yyyyMMdd"), number.Substring(4, 8));
    }

    [Fact]
    public void Generate_UsesDateFromClock()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 6, 18, 23, 59, 0, TimeSpan.Zero));
        var sut = new AccountNumberGenerator(clock);
        Assert.StartsWith("ACC-20260618-", sut.Generate());
    }

    [Fact]
    public void Generate_ProducesVariedNumbers_BasicUniqueness()
    {
        var sut = new AccountNumberGenerator(TimeProvider.System);
        var distinct = Enumerable.Range(0, 50).Select(_ => sut.Generate()).Distinct().Count();        
        Assert.True(distinct > 1);
    }
}
