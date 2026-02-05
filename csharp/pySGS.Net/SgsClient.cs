using Microsoft.Data.Analysis;

namespace PySgs;

public sealed class SgsClient
{
    private readonly ApiClient _apiClient = new();
    private readonly SearchService _searchService = new();

    public async Task<IReadOnlyList<TimeSeriesValue>> TimeSerieAsync(int tsCode, string start, string end, bool strict = false, CancellationToken cancellationToken = default)
    {
        var data = strict
            ? await _apiClient.GetDataWithStrictRangeAsync(tsCode, start, end, cancellationToken)
            : await _apiClient.GetDataAsync(tsCode, start, end, cancellationToken);

        return data
            .Select(point => new TimeSeriesValue(tsCode, point.ParsedDate, point.ParsedValue, point.Date, point.Value))
            .ToList();
    }


    public async Task<DataFrame> DataFrameAsync(IEnumerable<int> tsCodes, string start, string end, bool strict = false, CancellationToken cancellationToken = default)
    {
        var codes = tsCodes.Distinct().ToList();
        var indexedValues = new SortedDictionary<DateTime, Dictionary<int, double?>>();

        foreach (var code in codes)
        {
            var series = await TimeSerieAsync(code, start, end, strict, cancellationToken);
            foreach (var point in series)
            {
                if (!point.Date.HasValue)
                {
                    continue;
                }

                var date = point.Date.Value;
                if (!indexedValues.TryGetValue(date, out var row))
                {
                    row = new Dictionary<int, double?>();
                    indexedValues[date] = row;
                }

                row[code] = point.Value;
            }
        }

        var dateColumn = new PrimitiveDataFrameColumn<DateTime>("Date", indexedValues.Keys);
        var frame = new DataFrame(dateColumn);

        foreach (var code in codes)
        {
            var values = indexedValues
                .Select(kv => kv.Value.TryGetValue(code, out var v) ? v : null)
                .ToList();
            frame.Columns.Add(new PrimitiveDataFrameColumn<double?>($"{code}", values));
        }

        return frame;
    }

    public Task<DataFrame> DataFrameAsync(int tsCode, string start, string end, bool strict = false, CancellationToken cancellationToken = default)
        => DataFrameAsync(new[] { tsCode }, start, end, strict, cancellationToken);

    public async Task<IReadOnlyList<SearchResult?>> MetadataAsync(IEnumerable<int> tsCodes, string language = "en", CancellationToken cancellationToken = default)
    {
        var result = new List<SearchResult?>();
        foreach (var code in tsCodes)
        {
            var metadata = await _searchService.SearchTimeSeriesAsync(code, language, cancellationToken);
            result.Add(metadata?.FirstOrDefault());
        }

        return result;
    }

    public Task<IReadOnlyList<SearchResult?>> MetadataAsync(IEnumerable<int> tsCodes, Language language = Language.En, CancellationToken cancellationToken = default)
        => MetadataAsync(tsCodes, language.ToCode(), cancellationToken);

    public Task<IReadOnlyList<SearchResult?>> MetadataAsync(DataFrame frame, string language = "en", CancellationToken cancellationToken = default)
    {
        var tsCodes = frame.Columns
            .Select(c => c.Name)
            .Where(n => n != "Date")
            .Select(name => int.TryParse(name, out var code) ? code : (int?)null)
            .Where(code => code.HasValue)
            .Select(code => code!.Value);

        return MetadataAsync(tsCodes, language, cancellationToken);
    }

    public Task<IReadOnlyList<SearchResult?>> MetadataAsync(DataFrame frame, Language language = Language.En, CancellationToken cancellationToken = default)
        => MetadataAsync(frame, language.ToCode(), cancellationToken);

    public Task<IReadOnlyList<SearchResult>?> SearchAsync(int code, string language = "en", CancellationToken cancellationToken = default)
        => _searchService.SearchTimeSeriesAsync(code, language, cancellationToken);

    public Task<IReadOnlyList<SearchResult>?> SearchAsync(string text, string language = "en", CancellationToken cancellationToken = default)
        => _searchService.SearchTimeSeriesAsync(text, language, cancellationToken);

    public Task<IReadOnlyList<SearchResult>?> SearchAsync(int code, Language language = Language.En, CancellationToken cancellationToken = default)
        => SearchAsync(code, language.ToCode(), cancellationToken);

    public Task<IReadOnlyList<SearchResult>?> SearchAsync(string text, Language language = Language.En, CancellationToken cancellationToken = default)
        => SearchAsync(text, language.ToCode(), cancellationToken);
}
