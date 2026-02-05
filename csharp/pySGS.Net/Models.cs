namespace PySgs;

public enum Language
{
    Pt,
    En
}

public static class LanguageExtensions
{
    public static string ToCode(this Language language)
        => language == Language.Pt ? "pt" : "en";
}

public sealed record SearchResult(
    int Code,
    string Name,
    string Unit,
    string Frequency,
    object? FirstValue,
    object? LastValue,
    string Source);

public sealed record TimeSeriesPoint(string Date, string Value)
{
    public DateTime? ParsedDate => Common.TryParseDate(Date, out var dt) ? dt : null;

    public double? ParsedValue => Common.TryParseNumeric(Value, out var value) ? value : null;
}

public sealed record TimeSeriesValue(int Code, DateTime? Date, double? Value, string RawDate, string RawValue);
