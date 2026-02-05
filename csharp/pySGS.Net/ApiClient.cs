using System.Net;
using System.Text.Json;

namespace PySgs;

public sealed class ApiClient
{
    private static readonly HttpClient Http = new();

    public async Task<IReadOnlyList<TimeSeriesPoint>> GetDataAsync(int tsCode, string begin, string end, CancellationToken cancellationToken = default)
    {
        var encodedBegin = WebUtility.UrlEncode(begin);
        var encodedEnd = WebUtility.UrlEncode(end);
        var url = $"https://api.bcb.gov.br/dados/serie/bcdata.sgs.{tsCode}/dados?formato=json&dataInicial={encodedBegin}&dataFinal={encodedEnd}";

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var response = await Http.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var payload = await JsonSerializer.DeserializeAsync<List<SgsApiRecord>>(stream, cancellationToken: cancellationToken)
                    ?? new List<SgsApiRecord>();

                return payload.Select(r => new TimeSeriesPoint(r.Data, r.Valor)).ToList();
            }
            catch when (attempt < Common.MaxAttemptNumber)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }
    }

    public async Task<IReadOnlyList<TimeSeriesPoint>> GetDataWithStrictRangeAsync(int tsCode, string begin, string end, CancellationToken cancellationToken = default)
    {
        var data = await GetDataAsync(tsCode, begin, end, cancellationToken);
        if (data.Count == 0)
        {
            return data;
        }

        if (!Common.TryParseDate(data[0].Date, out var firstDate) || !Common.TryParseDate(begin, out var startDate))
        {
            return Array.Empty<TimeSeriesPoint>();
        }

        return firstDate < startDate ? Array.Empty<TimeSeriesPoint>() : data;
    }

    private sealed record SgsApiRecord(string Data, string Valor);
}
