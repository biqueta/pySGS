using System.Globalization;
using System.Text.RegularExpressions;

namespace PySgs;

public static class Common
{
    public const int MaxAttemptNumber = 5;

    private static readonly Regex YearRegex = new("^[0-9]{4}$", RegexOptions.Compiled);
    private static readonly Regex MonthYearRegex = new("^[a-z]{3}/[0-9]{4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, int> MonthMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jan"] = 1,
        ["fev"] = 2,
        ["feb"] = 2,
        ["mar"] = 3,
        ["abr"] = 4,
        ["apr"] = 4,
        ["mai"] = 5,
        ["may"] = 5,
        ["jun"] = 6,
        ["jul"] = 7,
        ["ago"] = 8,
        ["aug"] = 8,
        ["set"] = 9,
        ["sep"] = 9,
        ["out"] = 10,
        ["oct"] = 10,
        ["nov"] = 11,
        ["dez"] = 12,
        ["dec"] = 12
    };

    public static object ToDateTime(string dateString)
        => TryParseDate(dateString, out var dt) ? dt : dateString;

    public static bool TryParseDate(string? dateString, out DateTime value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return false;
        }

        if (YearRegex.IsMatch(dateString))
        {
            value = new DateTime(int.Parse(dateString, CultureInfo.InvariantCulture), 12, 31);
            return true;
        }

        if (MonthYearRegex.IsMatch(dateString))
        {
            var parts = dateString.Split('/');
            if (MonthMap.TryGetValue(parts[0], out var month))
            {
                value = new DateTime(int.Parse(parts[1], CultureInfo.InvariantCulture), month, 1);
                return true;
            }
        }

        return DateTime.TryParseExact(
            dateString,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out value);
    }

    public static bool TryParseNumeric(string? text, out double? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out var ptParsed) ||
            double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out ptParsed))
        {
            value = ptParsed;
            return true;
        }

        return false;
    }
}
