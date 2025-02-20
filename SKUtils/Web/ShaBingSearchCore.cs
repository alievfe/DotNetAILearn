using System.Globalization;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using AngleSharp.Dom;
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
            .Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false })
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

                    // TODO开头直接可能有 ans解答项目，有多种情况
                    var liAns = bResults.QuerySelector("li.b_ans.b_top.b_topborder");
                    var webPage = ParseTopAnswer(liAns);
                    if (webPage is not null)
                    {
                        results.Add(webPage);
                        if (results.Count >= searchOptions.Top)
                            break;
                    }

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
                        // 标题和url
                        var title = li.QuerySelector("h2 a")?.TextContent.Trim();
                        var iurl = urlNode?.GetAttribute("href")?.Trim();
                        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(iurl))
                            continue;
                        // 网站名
                        var siteName = urlNode?.GetAttribute("aria-label")?.Trim();

                        // 网站图标
                        var iconUrl = li.QuerySelector("div.wr_fav div img")
                            ?.GetAttribute("src")
                            ?.Trim();

                        // 获取网站内容，并分割出日期。可能多种情况
                        //var abstractText = li.QuerySelector("div.b_caption")?.TextContent.Trim();
                        var abstractText = li.QuerySelector("div.b_caption p.b_lineclamp2")
                            ?.TextContent.Trim();
                        if (string.IsNullOrEmpty(abstractText))
                        {
                            abstractText =
                                li.QuerySelector("div.b_caption p.b_lineclamp3")?.TextContent.Trim()
                                + li.QuerySelector("div.b_caption div.b_factrow div.b_vlist2col")
                                    ?.TextContent.Trim();
                        }
                        if (string.IsNullOrEmpty(abstractText)) // 知乎回答排在前面
                        {
                            abstractText = li.QuerySelector("div.b_algoQuizGoBig")
                                ?.TextContent.Trim();
                        }
                        var (dateLastCrawled, trimmedAbstract) = ParseDateAndTrimText(abstractText);

                        // 图像获取，可能有多种情况，左侧大图、右侧小图
                        var imageUrl = li.QuerySelector(
                                "div.mc_vtvc_con_rc div.b_canvas div.cico img"
                            )
                            ?.GetAttribute("src")
                            ?.Trim();
                        if (string.IsNullOrEmpty(imageUrl))
                            imageUrl = ParseImageUrlInLable(
                                li.QuerySelector("a.b_ci_image_ova")
                                    ?.GetAttribute("aria-label")
                                    ?.Trim()
                            );

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
                    Console.WriteLine($"Error: {ex}");
                    _logger.LogError($"Error: {ex}");
                    break;
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
        var response = new ShaBingSearchResponse<ShaBingWebPage>
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
        Console.WriteLine("\n\n----log------");
        Console.WriteLine("\n----查询输入 " + query);
        Console.WriteLine("\n----查询条件 " + JsonSerializer.Serialize(searchOptions));
        Console.WriteLine(
            "\n----搜索内容 "
                + JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true,
                    }
                )
        );
        Console.WriteLine("\n----log------\n");
        return response;
    }

    private ShaBingWebPage? ParseTopAnswer(IElement? liAns)
    {
        if (liAns is null)
            return null;
        var urlNode = liAns.QuerySelector("h2 a");
        var title = urlNode?.TextContent?.Trim();
        var iurl = urlNode?.GetAttribute("href")?.Trim();
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(iurl))
            return null;
        var siteName = liAns.QuerySelector("div.b_imagePair cite")?.TextContent?.Trim();
        var iconUrl = liAns.QuerySelector("div.b_imagePair img")?.GetAttribute("src")?.Trim();
        var abstractText = liAns.QuerySelector("div.rwrl")?.TextContent?.Trim();
        return new ShaBingWebPage
        {
            Id = "1",
            Name = title,
            DisplayUrl = iurl,
            Url = iurl,
            Snippet = abstractText,
            SiteIcon = iconUrl,
            SiteName = siteName,
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
