using System.Globalization;
using System.Net;
using System.Web;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using Microsoft.SemanticKernel.Data;

namespace SKUtils.Web;

public class ShaBingSearchCore : IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private int _timeout = 2500;
    private readonly string _host = "https://cn.bing.com";
    private readonly ILogger _logger;

    public ShaBingSearchCore(string? host = null, int? timeout = null, ILogger? logger = null)
    {
        _host = host ?? _host;
        _timeout = timeout ?? _timeout;
        _logger = logger ?? NullLogger.Instance;
        _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        _browser = _playwright
            .Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true })
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// 执行 Sha Bing 搜索查询并返回结果。
    /// </summary>
    /// <param name="query">要搜索的内容。</param>
    /// <param name="searchOptions">搜索选项。</param>
    /// <param name="cancellationToken">用于监视取消请求的 <see cref="CancellationToken"/>。默认值为 <see cref="CancellationToken.None"/>。</param>
    public async Task<ShaBingSearchResponse<ShaBingWebPage>?> ExecuteSearchAsync(
        string query,
        TextSearchOptions searchOptions,
        CancellationToken cancellationToken = default
    )
    {
        var results = new List<ShaBingWebPage>();
        long? totalEstimatedMatches = null;

        if (string.IsNullOrEmpty(query) || searchOptions.Top <= 0)
            return null;

        var page = await _browser.NewPageAsync();
        try
        {
            while (results.Count < searchOptions.Top)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var queryParams = new Dictionary<string, string>
                {
                    { "q", query },
                    { "FPIG", Guid.NewGuid().ToString("N") },
                    { "first", (searchOptions.Skip + results.Count).ToString() },
                    { "FORM", "PERE1" },
                };
                string url = $"{_host}/search?{UrlEncode(queryParams)}";

                try
                {
                    string? content = null;
                    try
                    {
                        await page.GotoAsync(
                            url,
                            new PageGotoOptions
                            {
                                WaitUntil = WaitUntilState.NetworkIdle,
                                Timeout = _timeout,
                            }
                        );
                        content = await page.ContentAsync();
                        if (content.Length < 100)
                        {
                            await page.ReloadAsync(
                                new PageReloadOptions
                                {
                                    WaitUntil = WaitUntilState.NetworkIdle,
                                    Timeout = _timeout,
                                }
                            );
                            content = await page.ContentAsync();
                        }
                    }
                    catch (TimeoutException)
                    {
                        content = await page.ContentAsync();
                    }

                    var parser = new HtmlParser();
                    var document = await parser.ParseDocumentAsync(content);

                    var bResults = document.QuerySelector("#b_results");
                    if (bResults == null)
                    {
                        _logger.LogInformation("No search results found.");
                        break;
                    }
                    totalEstimatedMatches = ParseTotalCountFromText(
                        document.QuerySelector("#b_tween_searchResults span")?.TextContent.Trim()
                    );

                    var resultItems = bResults.QuerySelectorAll("li.b_algo");
                    if (resultItems.Length == 0)
                    {
                        _logger.LogInformation("No result items on current page.");
                        break;
                    }

                    foreach (var li in resultItems)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var urlNode = li.QuerySelector("div.b_tpcn a");
                        var title = li.QuerySelector("h2 a")?.TextContent.Trim();
                        var iurl = urlNode?.GetAttribute("href")?.Trim();
                        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(iurl))
                            continue;

                        var siteName = urlNode?.GetAttribute("aria-label")?.Trim();
                        var iconUrl = li.QuerySelector("div.wr_fav div img")
                            ?.GetAttribute("src")
                            ?.Trim();
                        var imageUrl = ParseImageUrlInLable(
                            li.QuerySelector("a.b_ci_image_ova")?.GetAttribute("aria-label")?.Trim()
                        );
                        var abstractText = li.QuerySelector("div.b_caption")?.TextContent.Trim();
                        if (string.IsNullOrEmpty(abstractText))
                            abstractText = li.QuerySelector("p.b_lineclamp3")?.TextContent.Trim();
                        var (dateLastCrawled, trimmedAbstract) = ParseDateAndTrimText(abstractText);

                        results.Add(
                            new ShaBingWebPage
                            {
                                Id = (results.Count + 1).ToString(),
                                Name = title,
                                DisplayUrl = iurl,
                                Url = iurl,
                                Snippet = trimmedAbstract,
                                SiteIcon = iconUrl,
                                SiteName = siteName,
                                SiteImage = imageUrl,
                                DateLastCrawled = dateLastCrawled,
                            }
                        );

                        if (results.Count >= searchOptions.Top)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error: {ex}");
                    break;
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
        return new ShaBingSearchResponse<ShaBingWebPage>
        {
            Type = "SearchResponse",
            QueryContext = new ShaBingQueryContext { OriginalQuery = query },
            WebPages = new ShaBingWebPages<ShaBingWebPage>
            {
                Id = Guid.NewGuid().ToString(),
                TotalEstimatedMatches = totalEstimatedMatches ?? 0,
                WebSearchUrl = $"{_host}/search?q={WebUtility.UrlEncode(query)}",
                Value = results,
            },
        };
    }

    private long? ParseTotalCountFromText(string? input)
    {
        if (input is null)
            return null;
        string cleanNumber = input.Replace("约 ", "").Replace(" 个结果", "").Replace(",", "");
        if (long.TryParse(cleanNumber, out long number))
            return number;
        return null;
    }

    private string? ParseImageUrlInLable(string? lable)
    {
        if (lable is null)
            return null;
        string decodedUrl = WebUtility.HtmlDecode(lable);
        var uri = new Uri(_host + decodedUrl);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        return queryParams["mediaurl"];
    }

    private (string? ParsedDate, string? TrimmedInput) ParseDateAndTrimText(string? input)
    {
        if (input is null)
        {
            return (null, null);
        }

        string trimmedInput = input;
        int dotIndex = input.IndexOf(" · ");
        string? datePart;
        // 在前20个字符中找到，则说明存在日期片段，那么截取前面的为日期片段，后面的为实际文本；否则直接返回原始文本
        if (dotIndex != -1 && dotIndex < 20)
        {
            datePart = input[..dotIndex].Trim();
            trimmedInput = input[(dotIndex + 3)..];
        }
        else
        {
            return (null, trimmedInput);
        }
        // 1天之前，3天前
        if (datePart.EndsWith("天前") || datePart.EndsWith("天之前"))
        {
            int days = int.Parse(datePart.Replace("天前", "").Replace("天之前", "").Trim());
            return (
                DateOnly.FromDateTime(DateTime.Today).AddDays(-days).ToString("yyyy-MM-dd"),
                trimmedInput
            );
        }
        // 3小时之前
        if (datePart.EndsWith("之前"))
        {
            return (DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"), trimmedInput);
        }
        // 2000年1月1日
        if (
            DateOnly.TryParseExact(
                datePart,
                "yyyy年M月d日",
                null,
                DateTimeStyles.None,
                out var parsedDate
            )
        )
        {
            return (parsedDate.ToString("yyyy-MM-dd"), trimmedInput);
        }
        return (null, trimmedInput);
    }

    private string UrlEncode(Dictionary<string, string> parameters)
    {
        return string.Join(
            "&",
            parameters.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"
            )
        );
    }

    public void Dispose()
    {
        _browser?.CloseAsync()?.GetAwaiter().GetResult();
        _browser?.DisposeAsync().GetAwaiter().GetResult();
        _playwright?.Dispose();
    }
}
