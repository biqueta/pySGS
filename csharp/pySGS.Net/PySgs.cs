using Microsoft.Data.Analysis;

namespace PySgs;

/// <summary>
/// Python-like static API helpers for parity with the original pySGS package.
/// </summary>
public static class PySgsApi
{
    private static readonly SgsClient Client = new();

    public static Task<IReadOnlyList<TimeSeriesValue>> TimeSerieAsync(
        int tsCode,
        string start,
        string end,
        bool strict = false,
        CancellationToken cancellationToken = default)
        => Client.TimeSerieAsync(tsCode, start, end, strict, cancellationToken);

    public static Task<DataFrame> DataFrameAsync(
        IEnumerable<int> tsCodes,
        string start,
        string end,
        bool strict = false,
        CancellationToken cancellationToken = default)
        => Client.DataFrameAsync(tsCodes, start, end, strict, cancellationToken);

    public static Task<IReadOnlyList<SearchResult?>> MetadataAsync(
        IEnumerable<int> tsCodes,
        Language language = Language.En,
        CancellationToken cancellationToken = default)
        => Client.MetadataAsync(tsCodes, language.ToCode(), cancellationToken);

    public static Task<IReadOnlyList<SearchResult?>> MetadataAsync(
        DataFrame frame,
        Language language = Language.En,
        CancellationToken cancellationToken = default)
        => Client.MetadataAsync(frame, language.ToCode(), cancellationToken);

    public static Task<IReadOnlyList<SearchResult>?> SearchTsAsync(
        int code,
        Language language = Language.En,
        CancellationToken cancellationToken = default)
        => Client.SearchAsync(code, language.ToCode(), cancellationToken);

    public static Task<IReadOnlyList<SearchResult>?> SearchTsAsync(
        string text,
        Language language = Language.En,
        CancellationToken cancellationToken = default)
        => Client.SearchAsync(text, language.ToCode(), cancellationToken);
}
