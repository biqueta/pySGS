# pySGS C# Port — Status da Conversão

Este diretório contém a migração progressiva da biblioteca Python `pySGS` para C#/.NET.

> Objetivo final: disponibilizar uma biblioteca C# com paridade funcional com a API pública Python (`time_serie`, `dataframe`, `metadata`, `search_ts`) e cobertura de testes equivalente.

---

## 1) O que já foi convertido

### Estrutura de projeto

- ✅ Biblioteca principal: `csharp/pySGS.Net/pySGS.Net.csproj`.
- ✅ Solution .NET: `csharp/pySGS.sln`.
- ✅ Projeto de testes: `csharp/pySGS.Net.Tests/pySGS.Net.Tests.csproj`.

### Domínio e contratos

- ✅ `Language` (`pt`/`en`) e extensão para código textual.
- ✅ Modelos de domínio:
  - `SearchResult`
  - `TimeSeriesPoint`
  - `TimeSeriesValue`

### Funcionalidades principais

- ✅ Cliente de API SGS (`ApiClient`):
  - chamada ao endpoint JSON da série;
  - codificação de parâmetros de data;
  - retries para falhas transitórias;
  - comportamento de intervalo estrito (`GetDataWithStrictRangeAsync`).
- ✅ Serviço de busca (`SearchService`):
  - busca por código e por texto;
  - parse da tabela HTML `tabelaSeries`;
  - suporte a português e inglês.
- ✅ Fachada OO (`SgsClient`):
  - `TimeSerieAsync`
  - `DataFrameAsync`
  - `MetadataAsync`
  - `SearchAsync`
- ✅ Fachada estática (`PySgsApi`) no estilo da API Python.

### Utilitários

- ✅ Parsing de datas SGS em `Common`:
  - `yyyy`
  - `mmm/yyyy` (pt e en)
  - `dd/MM/yyyy`
- ✅ Parsing numérico com suporte a formato pt-BR.

### Testes já implementados

- ✅ Testes de utilitários (`CommonTests`): parsing de datas e números.
- ✅ Testes de parsing de busca (`SearchServiceTests`):
  - resultado não encontrado;
  - tabela PT;
  - tabela EN.

---

## 2) O que ainda falta para concluir a conversão total

Abaixo está o backlog principal para atingir paridade completa com o projeto Python.

### A. Paridade de comportamento com Python

- 🔲 Revisar 100% da semântica de `strict=True` para reproduzir mensagens/retornos do Python em casos limítrofes.
- 🔲 Garantir compatibilidade de tipos retornados com o esperado por consumidores (ex.: datas não parseáveis retornando string quando aplicável).
- 🔲 Revalidar mapeamento de colunas e variações de HTML do SGS (mudanças de layout/tabela).

### B. API pública e ergonomia

- 🔲 Definir API final para publicação (manter apenas fachada OO, estática ou ambas).
- 🔲 Adicionar XML docs completos para todos os métodos públicos.
- 🔲 Padronizar nomenclatura para facilitar adoção em C# idiomático sem perder paridade com Python.

### C. DataFrame e manipulação de dados

- 🔲 Revisar estratégia de alinhamento e tipos de coluna no `Microsoft.Data.Analysis` para datasets grandes.
- 🔲 Adicionar cenários de desempenho e memória (séries longas/múltiplas).
- 🔲 Validar comportamento com valores ausentes e séries com periodicidades diferentes.

### D. Testes e qualidade

- 🔲 Aumentar cobertura de testes unitários para:
  - `ApiClient` (mock de `HttpMessageHandler`);
  - `SgsClient` (fluxos de integração interna);
  - cenários de erro/retry/timeouts.
- 🔲 Criar testes de integração opcionais (com flag) para ambiente com internet.
- 🔲 Definir meta mínima de cobertura e pipeline CI para build/test (GitHub Actions ou equivalente).

### E. Empacotamento e distribuição

- 🔲 Configurar metadados de pacote NuGet (ícone, readme do pacote, tags, repositório, SourceLink).
- 🔲 Definir versionamento semântico e changelog.
- 🔲 Publicar pacote pré-release e validar consumo em projeto exemplo.

### F. Observabilidade e robustez

- 🔲 Melhorar estratégia de retry (ex.: política por status code, jitter/backoff mais robusto).
- 🔲 Adicionar suporte a `ILogger`/telemetria opcional.
- 🔲 Avaliar suporte a `HttpClient` injetável para testabilidade e customização.

---

## 3) Mapeamento Python ➜ C# (estado atual)

| Python (`sgs`) | C# (`PySgs`) | Status |
|---|---|---|
| `common.to_datetime` | `Common.ToDateTime` / `TryParseDate` | ✅ Parcialmente equivalente |
| `api.get_data` | `ApiClient.GetDataAsync` | ✅ |
| `api.get_data_with_strict_range` | `ApiClient.GetDataWithStrictRangeAsync` | ✅ (revisão fina pendente) |
| `search.search_ts` | `SearchService.SearchTimeSeriesAsync` / `SgsClient.SearchAsync` | ✅ |
| `metadata.metadata` | `SgsClient.MetadataAsync` | ✅ |
| `ts.time_serie` | `SgsClient.TimeSerieAsync` | ✅ |
| `dataframe.dataframe` | `SgsClient.DataFrameAsync` | ✅ |
| API pública via `__init__` | `PySgsApi` | ✅ Inicial |

> Observação: a linha “Parcialmente equivalente” indica que a função existe e cobre os casos principais, mas ainda requer validação de todas as bordas do comportamento original.

---

## 4) Como contribuir na próxima etapa

1. Priorizar paridade de comportamento (itens A + D).
2. Fechar testabilidade do `ApiClient` com injeção de handler/client.
3. Estabilizar API pública final e documentação XML.
4. Preparar empacotamento NuGet e CI.

---

## 5) Comandos esperados (ambiente com .NET SDK)

```bash
dotnet restore csharp/pySGS.sln
dotnet build csharp/pySGS.sln -c Release
dotnet test csharp/pySGS.sln -c Release
```

No ambiente atual desta automação, o SDK .NET pode não estar instalado.
