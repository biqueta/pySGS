using Xunit;

namespace PySgs.Tests;

public class SearchServiceTests
{
    [Fact]
    public void ParseSearchResponse_ReturnsNullWhenNotFound()
    {
        const string html = "<html><body>Nenhuma série localizada</body></html>";

        var results = SearchService.ParseSearchResponse(html, "pt");

        Assert.Null(results);
    }

    [Fact]
    public void ParseSearchResponse_ParsesPortugueseTable()
    {
        const string html = @"<table id='tabelaSeries'>
<tr>
  <th>Cód.</th><th>Nome completo</th><th>Unid.</th><th>Per.</th><th>Início  dd/MM/aaaa</th><th>Últ. valor</th><th>Fonte</th>
</tr>
<tr>
  <td>12</td><td>CDI</td><td>%</td><td>D</td><td>06/03/1986</td><td>31/12/2020</td><td>Cetip</td>
</tr>
</table>";

        var results = SearchService.ParseSearchResponse(html, "pt");

        Assert.NotNull(results);
        Assert.Single(results!);
        Assert.Equal(12, results[0].Code);
        Assert.Equal("CDI", results[0].Name);
        Assert.Equal("D", results[0].Frequency);
        Assert.Equal("Cetip", results[0].Source);
        Assert.IsType<DateTime>(results[0].FirstValue!);
        Assert.IsType<DateTime>(results[0].LastValue!);
    }

    [Fact]
    public void ParseSearchResponse_ParsesEnglishTable()
    {
        const string html = @"<table id='tabelaSeries'>
<tr>
  <th>Code</th><th>Full name</th><th>Unit</th><th>Per.</th><th>Start  dd/MM/yyyy</th><th>Last value</th><th>Source</th>
</tr>
<tr>
  <td>4</td><td>Gold</td><td>c.m.u.</td><td>D</td><td>29/12/1989</td><td>31/12/2020</td><td>BM&amp;FBOVESPA</td>
</tr>
</table>";

        var results = SearchService.ParseSearchResponse(html, "en");

        Assert.NotNull(results);
        Assert.Single(results!);
        Assert.Equal(4, results[0].Code);
        Assert.Equal("Gold", results[0].Name);
        Assert.Equal("BM&FBOVESPA", results[0].Source);
    }
}
