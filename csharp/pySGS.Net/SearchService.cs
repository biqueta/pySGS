using HtmlAgilityPack;
using System.Globalization;

namespace PySgs;

public sealed class SearchService
{
    private static readonly HttpClient Http = new(new HttpClientHandler { UseCookies = true });

    private static readonly Dictionary<string, string> SearchUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pt"] = "https://www3.bcb.gov.br/sgspub/index.jsp?idIdioma=P",
        ["en"] = "https://www3.bcb.gov.br/sgspub/"
    };

    public Task<IReadOnlyList<SearchResult>?> SearchTimeSeriesAsync(int query, string language = "en", CancellationToken cancellationToken = default)
        => SearchCoreAsync(query.ToString(CultureInfo.InvariantCulture), byCode: true, language, cancellationToken);

    public Task<IReadOnlyList<SearchResult>?> SearchTimeSeriesAsync(string query, string language = "en", CancellationToken cancellationToken = default)
        => SearchCoreAsync(query, byCode: false, language, cancellationToken);

    private async Task<IReadOnlyList<SearchResult>?> SearchCoreAsync(string query, bool byCode, string language, CancellationToken cancellationToken)
    {
        language = language.ToLowerInvariant();
        if (!SearchUrls.TryGetValue(language, out var searchUrl))
        {
            throw new ArgumentException("language must be en or pt", nameof(language));
        }

        var method = byCode ? "localizarSeriesPorCodigo" : "localizarSeriesPorTexto";
        var endpoint = "https://www3.bcb.gov.br/sgspub/localizarseries/localizarSeries.do";

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await Http.GetAsync(searchUrl, cancellationToken);

                var formData = new Dictionary<string, string?>
                {
                    ["method"] = method,
                    ["periodicidade"] = "0",
                    ["codigo"] = byCode ? query : null,
                    ["fonte"] = "341",
                    ["texto"] = byCode ? null : query,
                    ["hdFiltro"] = null,
                    ["hdOidGrupoSelecionado"] = null,
                    ["hdSeqGrupoSelecionado"] = null,
                    ["hdNomeGrupoSelecionado"] = null,
                    ["hdTipoPesquisa"] = byCode ? "4" : "6",
                    ["hdTipoOrdenacao"] = "0",
                    ["hdNumPagina"] = null,
                    ["hdPeriodicidade"] = "Todas",
                    ["hdSeriesMarcadas"] = null,
                    ["hdMarcarTodos"] = null,
                    ["hdFonte"] = null,
                    ["hdOidSerieMetadados"] = null,
                    ["hdNumeracao"] = null,
                    ["hdOidSeriesLocalizadas"] = null,
                    ["linkRetorno"] = "/sgspub/consultarvalores/telaCvsSelecionarSeries.paint",
                    ["linkCriarFiltros"] = "/sgspub/manterfiltros/telaMfsCriarFiltro.paint"
                };

                using var content = new FormUrlEncodedContent(
                    formData.Where(kv => kv.Value is not null)
                        .Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value!)));

                var response = await Http.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync(cancellationToken);
                return ParseSearchResponse(html, language);
            }
            catch when (attempt < Common.MaxAttemptNumber)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }
    }

    public static IReadOnlyList<SearchResult>? ParseSearchResponse(string html, string language)
    {
        if (html.Contains("No series found", StringComparison.OrdinalIgnoreCase) || html.Contains("Nenhuma série localizada", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var cols = language.Equals("pt", StringComparison.OrdinalIgnoreCase)
            ? new { Start = "Início  dd/MM/aaaa", Last = "Últ. valor", Code = "Cód.", Frequency = "Per.", Name = "Nome completo", Source = "Fonte", Unit = "Unid." }
            : new { Start = "Start  dd/MM/yyyy", Last = "Last value", Code = "Code", Frequency = "Per.", Name = "Full name", Source = "Source", Unit = "Unit" };

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var table = doc.DocumentNode.SelectSingleNode("//table[@id='tabelaSeries']");
        if (table is null)
        {
            return null;
        }

        var rows = table.SelectNodes(".//tr");
        if (rows is null || rows.Count < 2)
        {
            return null;
        }

        List<string>? headers = null;
        var results = new List<SearchResult>();

        foreach (var row in rows)
        {
            var headerCells = row.SelectNodes(".//th|.//td")?.Select(h => HtmlEntity.DeEntitize(h.InnerText).Trim()).ToList();
            if (headerCells is null || headerCells.Count == 0)
            {
                continue;
            }

            if (headers is null)
            {
                if (headerCells.Contains(cols.Code))
                {
                    headers = headerCells;
                }
                continue;
            }

            var cells = row.SelectNodes(".//td")?.Select(c => HtmlEntity.DeEntitize(c.InnerText).Trim()).ToList();
            if (cells is null || cells.Count == 0 || cells.Count > headers.Count)
            {
                continue;
            }

            var map = headers.Zip(cells, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

            if (!map.TryGetValue(cols.Code, out var codeText) || !int.TryParse(codeText, NumberStyles.Any, CultureInfo.InvariantCulture, out var code))
            {
                continue;
            }

            var firstParsed = map.TryGetValue(cols.Start, out var startText) ? Common.ToDateTime(startText) : null;
            var lastParsed = map.TryGetValue(cols.Last, out var lastText) ? Common.ToDateTime(lastText) : null;

            results.Add(new SearchResult(
                code,
                map.GetValueOrDefault(cols.Name) ?? string.Empty,
                map.GetValueOrDefault(cols.Unit) ?? string.Empty,
                map.GetValueOrDefault(cols.Frequency) ?? string.Empty,
                firstParsed,
                lastParsed,
                map.GetValueOrDefault(cols.Source) ?? string.Empty));
        }

        return results.Count == 0 ? null : results;
    }
}
