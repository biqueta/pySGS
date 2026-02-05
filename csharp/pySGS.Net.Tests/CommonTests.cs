using Xunit;

namespace PySgs.Tests;

public class CommonTests
{
    [Theory]
    [InlineData("2019", 2019, 12, 31)]
    [InlineData("jan/2020", 2020, 1, 1)]
    [InlineData("fev/2020", 2020, 2, 1)]
    [InlineData("apr/2020", 2020, 4, 1)]
    [InlineData("31/12/2021", 2021, 12, 31)]
    public void TryParseDate_ParsesSupportedFormats(string input, int year, int month, int day)
    {
        var ok = Common.TryParseDate(input, out var date);

        Assert.True(ok);
        Assert.Equal(new DateTime(year, month, day), date);
    }

    [Fact]
    public void TryParseDate_ReturnsFalseForInvalidInput()
    {
        var ok = Common.TryParseDate("not-a-date", out _);
        Assert.False(ok);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("0", 0d)]
    [InlineData("1.23", 1.23d)]
    [InlineData("1,23", 1.23d)]
    public void TryParseNumeric_ParsesSgsNumericFormats(string input, double? expected)
    {
        var ok = Common.TryParseNumeric(input, out var value);

        Assert.True(ok);
        Assert.Equal(expected, value);
    }
}
