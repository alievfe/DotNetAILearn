##### c




```cs
// 版权所有 (c) 微软公司。保留所有权利。

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Data;

/// <summary>
/// 用于基于文本的搜索查询的接口，可与语义内核提示和自动函数调用配合使用。
/// </summary>
[Experimental("SKEXP0001")]
public interface ITextSearch
{
    /// <summary>
    /// 执行与指定查询相关的内容搜索，并返回表示搜索结果的 <see cref="string"/> 值。
    /// </summary>
    /// <param name="query">要搜索的内容。</param>
    /// <param name="searchOptions">执行文本搜索时使用的选项。</param>
    /// <param name="cancellationToken">用于监视取消请求的 <see cref="CancellationToken"/>。默认值为 <see cref="CancellationToken.None"/>。</param>
    public Task<KernelSearchResults<string>> SearchAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行与指定查询相关的内容搜索，并返回表示搜索结果的 <see cref="TextSearchResult"/> 值。
    /// </summary>
    /// <param name="query">要搜索的内容。</param>
    /// <param name="searchOptions">执行文本搜索时使用的选项。</param>
    /// <param name="cancellationToken">用于监视取消请求的 <see cref="CancellationToken"/>。默认值为 <see cref="CancellationToken.None"/>。</param>
    public Task<KernelSearchResults<TextSearchResult>> GetTextSearchResultsAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行与指定查询相关的内容搜索，并返回表示搜索结果的 <see cref="object"/> 值。
    /// </summary>
    /// <param name="query">要搜索的内容。</param>
    /// <param name="searchOptions">执行文本搜索时使用的选项。</param>
    /// <param name="cancellationToken">用于监视取消请求的 <see cref="CancellationToken"/>。默认值为 <see cref="CancellationToken.None"/>。</param>
    public Task<KernelSearchResults<object>> GetSearchResultsAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default);
}
```



```cs
// 版权所有 (c) 微软。保留所有权利。
using System.Diagnostics.CodeAnalysis;
namespace Microsoft.SemanticKernel.Data;
/// <summary>
/// 使用 <see cref="ITextSearch"/> 时可以应用的选项。
/// </summary>
[Experimental("SKEXP0001")]
public sealed class TextSearchOptions
{
/// <summary>
/// 默认返回的搜索结果数量。
/// </summary>
public static readonly int DefaultTop = 5;
/// <summary>
/// 指示结果中是否应包含总数的标志。
/// </summary>
/// <remarks>
/// 默认值为 false。
/// 并非所有文本搜索实现都支持此选项。
/// </remarks>
public bool IncludeTotalCount { get; init; } = false;
/// <summary>
/// 应用于搜索查询的筛选表达式。
/// </summary>
public TextSearchFilter? Filter { get; init; }
/// <summary>
/// 返回的搜索结果数量。
/// </summary>
public int Top { get; init; } = DefaultTop;
/// <summary>
/// 要返回的第一个结果的索引。
/// </summary>
public int Skip { get; init; } = 0;
}
```



```cs
// 版权所有 (c) 微软。保留所有权利。
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.VectorData;
namespace Microsoft.SemanticKernel.Data;
/// <summary>
/// 在使用 <see cref="ITextSearch"/> 时用于提供筛选功能。
/// </summary>
/// <remarks>
/// 一个筛选器包含一组 <see cref="FilterClause"/>，<see cref="ITextSearch"/> 实现可以使用这些子句，
/// 来要求底层搜索服务对搜索结果进行筛选。
/// </remarks>
[Experimental("SKEXP0001")]
public sealed class TextSearchFilter
{
    /// <summary>
    /// 应用于 <see cref="TextSearchFilter" /> 的子句。
    /// </summary>
    public IEnumerable<FilterClause> FilterClauses => this._filterClauses;
    /// <summary>
    /// 向筛选选项中添加一个相等性子句。
    /// </summary>
    /// <param name="字段名称">字段的名称。</param>
    /// <param name="值">字段的值。</param>
    /// <returns>筛选选项实例，以支持链式配置。</returns>
    public TextSearchFilter Equality(string fieldName, object value)
    {
        this._filterClauses.Add(new EqualToFilterClause(fieldName, value));
        return this;
    }
    #region 私有成员
        private readonly List<FilterClause> _filterClauses = [];
    #endregion
}

```



```cs
/// <summary>
/// 筛选子句的基类。
/// </summary>
/// <remarks>
/// <see cref="FilterClause"/> 用于要求底层搜索服务根据指定的条件对搜索结果进行筛选。
/// </remarks>
public abstract class FilterClause
{
    internal FilterClause()
    {
    }
}

```



```cs
// 版权所有 (c) 微软公司。保留所有权利。
namespace Microsoft.Extensions.VectorData;
/// <summary>
/// 使用字段值相等性进行过滤的 <see cref="FilterClause"/>。
/// </summary>
public sealed class EqualToFilterClause : FilterClause
{
    /// <summary>
    /// 初始化 <see cref="EqualToFilterClause"/> 类的新实例。
    /// </summary>
    /// <param name="fieldName">字段名。</param>
    /// <param name="value">字段值。</param>
    public EqualToFilterClause(string fieldName, object value)
    {
        this.FieldName = fieldName;
        this.Value = value;
    }
    /// <summary>
    /// 要匹配的字段名。
    /// </summary>
    public string FieldName { get; private set; }
    /// <summary>
    /// 要匹配的字段值。
    /// </summary>
    public object Value { get; private set; }
}
```



```cs
// 版权所有 (c) 微软公司。保留所有权利。

namespace Microsoft.Extensions.VectorData;

/// <summary>
/// 通过检查包含值列表的字段是否包含特定值来进行过滤的 <see cref="FilterClause"/>。
/// </summary>
public sealed class AnyTagEqualToFilterClause : FilterClause
{
    /// <summary>
    /// 初始化 <see cref="AnyTagEqualToFilterClause"/> 类的新实例。
    /// </summary>
    /// <param name="fieldName">包含值列表的字段名。</param>
    /// <param name="value">列表应包含的值。</param>
    public AnyTagEqualToFilterClause(string fieldName, string value)
    {
        this.FieldName = fieldName;
        this.Value = value;
    }

    /// <summary>
    /// 包含值列表的字段名。
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// 列表应包含的值。
    /// </summary>
    public string Value { get; private set; }
}
```



```cs
// 版权所有 (c) 微软公司。保留所有权利。

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Plugins.Web.Bing;

/// <summary>
/// 定义与查询相关的网页。
/// </summary>
public sealed class BingWebPage
{
    /// <summary>
    /// 仅允许在本包内创建。
    /// </summary>
    [JsonConstructorAttribute]
    internal BingWebPage()
    {
    }

    /// <summary>
    /// Bing 最后一次爬取该网页的时间。
    /// </summary>
    /// <remarks>
    /// 日期格式为 YYYY-MM-DDTHH:MM:SS。例如，2015-04-13T05:23:39。
    /// </remarks>
    [JsonPropertyName("dateLastCrawled")]
    public string? DateLastCrawled { get; set; }

    /// <summary>
    /// Bing 在包含此网页的网站中找到的相关内容链接列表。
    /// </summary>
    /// <remarks>
    /// 此上下文中的 BingWebPage 对象仅包含 name、url、urlPingSuffix 和 snippet 字段。
    /// </remarks>
    [JsonPropertyName("deepLinks")]
    public IReadOnlyList<BingWebPage>? DeepLinks { get; set; }

    /// <summary>
    /// 网页的显示 URL。
    /// </summary>
    /// <remarks>
    /// 此 URL 仅用于显示目的，格式可能不正确。
    /// </remarks>
    [JsonPropertyName("displayUrl")]
#pragma warning disable CA1056 // 类似 URI 的属性不应为字符串
    public string? DisplayUrl { get; set; }
#pragma warning restore CA1056 // 类似 URI 的属性不应为字符串

    /// <summary>
    /// 此网页在网页搜索结果列表中的唯一标识符。
    /// </summary>
    /// <remarks>
    /// 仅当 Ranking 答案指定要将网页与其他搜索结果混合时，对象才包含此字段。
    /// 每个网页都包含一个与 Ranking 答案中的 ID 匹配的 ID。有关更多信息，请参阅排名结果。
    /// </remarks>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 网页的名称。
    /// </summary>
    /// <remarks>
    /// 将此名称与 url 一起使用，创建一个超链接，用户点击该链接即可访问该网页。
    /// </remarks>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 网页所有者选择用于表示页面内容的图像的 URL。仅在可用时包含。
    /// </summary>
    [JsonPropertyName("openGraphImage")]
    public IReadOnlyList<BingOpenGraphImage>? OpenGraphImage { get; set; }

    /// <summary>
    /// 网页所有者在网页上指定的搜索标签列表。API 仅返回已索引的搜索标签。
    /// </summary>
    /// <remarks>
    /// MetaTag 对象的 name 字段包含已索引的搜索标签。搜索标签以 search.* 开头（例如，search.assetId）。content 字段包含标签的值。
    /// </remarks>
    [JsonPropertyName("searchTags")]
    public IReadOnlyList<BingMetaTag>? SearchTags { get; set; }

    /// <summary>
    /// 描述网页内容的网页文本片段。
    /// </summary>
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    /// <summary>
    /// 网页的 URL。
    /// </summary>
    /// <remarks>
    /// 将此 URL 与 name 一起使用，创建一个超链接，用户点击该链接即可访问该网页。
    /// </remarks>
    [JsonPropertyName("url")]
#pragma warning disable CA1056 // 类似 URI 的属性不应为字符串
    public string? Url { get; set; }
#pragma warning restore CA1056 // 类似 URI 的属性不应为字符串

    /// <summary>
    /// 标识网页使用的语言的双字母语言代码。例如，英语的语言代码是 en。
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// 一个布尔值，指示网页是否包含成人内容。如果网页不包含成人内容，则 isFamilyFriendly 设置为 true。
    /// </summary>
    [JsonPropertyName("isFamilyFriendly")]
    public bool? IsFamilyFriendly { get; set; }

    /// <summary>
    /// 一个布尔值，指示用户的查询是否经常用于导航到网页域名的不同部分。
    /// 如果用户从该页面导航到网站的其他部分，则为 true。
    /// </summary>
    [JsonPropertyName("isNavigational")]
    public bool? IsNavigational { get; set; }
}
```































































##### p

```
C#解释以下代码作用，同时给出案例结果
        Uri uri = new($"{this._uri}?q={BuildQuery(query, searchOptions)}");
private static string BuildQuery(string query, TextSearchOptions searchOptions)
    {
        StringBuilder fullQuery = new();
        fullQuery.Append(Uri.EscapeDataString(query.Trim()));
        StringBuilder queryParams = new();
        if (searchOptions.Filter is not null)
        {
            var filterClauses = searchOptions.Filter.FilterClauses;

            foreach (var filterClause in filterClauses)
            {
                if (filterClause is EqualToFilterClause equalityFilterClause)
                {
                    if (s_advancedSearchKeywords.Contains(equalityFilterClause.FieldName, StringComparer.OrdinalIgnoreCase) && equalityFilterClause.Value is not null)
                    {
                        fullQuery.Append($"+{equalityFilterClause.FieldName}%3A").Append(Uri.EscapeDataString(equalityFilterClause.Value.ToString()!));
                    }
                    else if (s_queryParameters.Contains(equalityFilterClause.FieldName, StringComparer.OrdinalIgnoreCase) && equalityFilterClause.Value is not null)
                    {
                        string? queryParam = s_queryParameters.FirstOrDefault(s => s.Equals(equalityFilterClause.FieldName, StringComparison.OrdinalIgnoreCase));
                        queryParams.Append('&').Append(queryParam!).Append('=').Append(Uri.EscapeDataString(equalityFilterClause.Value.ToString()!));
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown equality filter clause field name '{equalityFilterClause.FieldName}', must be one of {string.Join(",", s_queryParameters)},{string.Join(",", s_advancedSearchKeywords)}", nameof(searchOptions));
                    }
                }
            }
        }

        fullQuery.Append($"&count={searchOptions.Top}&offset={searchOptions.Skip}{queryParams}");

        return fullQuery.ToString();
    }
```





```
C#帮我重构代码，已有代码如下，现在要迁移到新的类，以及新的返回类型，同时优化调整代码逻辑，保证健全高可用。

public class SearchResult
{
    public int Rank { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public string? SiteName { get; set; }
    public string? SiteIcon { get; set; }
    public string? Image { get; set; }
    public DateOnly? SiteDate { get; set; }
    public string? Abstract { get; set; }
}
public class BingSearchTest : IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;

    private readonly string _host = "https://cn.bing.com";

    public BingSearchTest()
    {
        _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        _browser = _playwright
            .Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true })
            .GetAwaiter()
            .GetResult();
    }

    public async Task<List<SearchResult>> SearchAsync(
        string keyword,
        int numResults = 30,
        bool debug = false
    )
    {
        var results = new List<SearchResult>();

        if (string.IsNullOrEmpty(keyword) || numResults <= 0)
            return results;

        var page = await _browser.NewPageAsync();

        while (results.Count < numResults)
        {

            var queryParams = new Dictionary<string, string>
            {
                { "q", keyword },
                { "FPIG", Guid.NewGuid().ToString("N") },
                { "first", results.Count.ToString() },
                { "FORM", "PERE1" },
            };
            string url = $"{_host}/search?{UrlEncode(queryParams)}";

            try
            {
                string? content = null;
                try
                {
                    await page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = 2500
                    });
                    content = await page.ContentAsync();
                    if (content.Length < 100)
                    {
                        await page.ReloadAsync(
                            new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 2500 }
                        );
                        content = await page.ContentAsync();
                    }
                }
                catch (TimeoutException)
                {
                    content = await page.ContentAsync();
                }
                catch (Exception)
                {
                    throw;
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);

                var bResults = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='b_results']");
                if (bResults == null)
                {
                    if (debug)
                        Console.WriteLine("No search results found.");
                    break;
                }
                var totalCountNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='b_tween_searchResults']/span");
                long? totalEstimatedMatches;
                if (totalCountNode is not null)
                    totalEstimatedMatches = ParseTotalCountFromText(totalCountNode.InnerText.Trim());

                var resultItems = bResults.SelectNodes("./li[contains(@class, 'b_algo')]");
                if (resultItems == null || resultItems.Count == 0)
                {
                    if (debug)
                        Console.WriteLine("No result items on current page.");
                    break;
                }

                foreach (var li in resultItems)
                {
                    var titleNode = li.SelectSingleNode(".//h2/a");
                    var urlNode = li.SelectSingleNode(".//div[contains(@class, 'b_tpcn')]//a");
                    var iconNode = li.SelectSingleNode(".//div[contains(@class, 'wr_fav')]/div/img");
                    var abstractNode = li.SelectSingleNode(".//div[contains(@class, 'b_caption')]");
                    var abstractNode2 = li.SelectSingleNode(
                        ".//p[contains(@class, 'b_lineclamp3')]"
                    );
                    var imageNode = li.SelectSingleNode(".//a[contains(@class, 'b_ci_image_ova')]");
                    var title = titleNode?.InnerText.Trim();
                    var iurl = urlNode?.GetAttributeValue("href", "").Trim();
                    var siteName = urlNode?.GetAttributeValue("aria-label", "").Trim();
                    var iconurl = iconNode?.GetAttributeValue("src", "").Trim();

                    var abstractText = abstractNode?.InnerText.Trim();
                    if (string.IsNullOrEmpty(abstractText))
                        abstractText = abstractNode2?.InnerText.Trim() ?? "";
                    
                    string? imageurl = null;
                    if (imageNode is not null)
                        imageurl = ParseImageUrlInLable(
                            imageNode.GetAttributeValue("aria-label", "").Trim()
                        );

                    var siteDate = ParseDateInText(abstractText);

                    if (string.IsNullOrEmpty(title))
                        continue;
                    if (string.IsNullOrEmpty(iurl))
                        continue;

                    results.Add(
                        new SearchResult
                        {
                            Rank = results.Count + 1,
                            Title = title,
                            Url = iurl,
                            SiteName = siteName,
                            SiteIcon = iconurl,
                            SiteDate = siteDate,
                            Image = imageurl,
                            Abstract = abstractText,
                        }
                    );

                    if (results.Count >= numResults)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                break;
            }
        }

        return results.Take(numResults).ToList();
    }
    private long? ParseTotalCountFromText(string input)
    {
        string cleanNumber = input.Replace("约 ", "").Replace(" 个结果", "").Replace(",", "");
        // 尝试解析为 long 类型
        if (long.TryParse(cleanNumber, out long number))
            return number;
        else
            return null;
    }
    private string? ParseImageUrlInLable(string lable)
    {
        string decodedUrl = System.Net.WebUtility.HtmlDecode(lable);
        var uri = new Uri(_host + decodedUrl);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        return queryParams["mediaurl"];
    }

    private DateOnly? ParseDateInText(string input)
    {
        input = input.Length < 20 ? input : input[..20];
        int dotIndex = input.IndexOf(" · ");
        if (dotIndex == -1)
            return null;
        string datePart = input[..dotIndex].Trim();
        if (datePart.EndsWith("天前"))
        {
            int days = int.Parse(datePart.Replace("天前", "").Trim());
            return DateOnly.FromDateTime(DateTime.Today).AddDays(-days);
        }
        if (datePart.EndsWith("天之前"))
        {
            int days = int.Parse(datePart.Replace("天之前", "").Trim());
            return DateOnly.FromDateTime(DateTime.Today).AddDays(-days);
        }
        if (datePart.EndsWith("之前"))
        {
            return DateOnly.FromDateTime(DateTime.Today);
        }
        if (
            DateOnly.TryParseExact(
                datePart,
                "yyyy年M月d日",
                null,
                System.Globalization.DateTimeStyles.None,
                out var parsedDate
            )
        )
        {
            return parsedDate;
        }
        return null;
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


/////////// 新的类以及返回类型
internal class ShaBingSearchCore
{
    public async Task<ShaBingSearchResponse<ShaBingWebPage>?> ExecuteSearchAsync(
        string query,
        TextSearchOptions searchOptions,
        CancellationToken cancellationToken = default
    )
    {

        throw new NotImplementedException();
    }
}

#pragma warning disable CA1812 // 通过反射实例化
/// <summary>
/// Sha Bing 搜索响应。
/// </summary>
internal sealed class ShaBingSearchResponse<T>
{
    /// <summary>
    /// 类型提示，设置为 SearchResponse。
    /// </summary>
    [JsonPropertyName("_type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Bing 用于请求的查询字符串。
    /// </summary>
    [JsonPropertyName("queryContext")]
    public ShaBingQueryContext? QueryContext { get; set; }

    /// <summary>
    /// 一个可空的 WebAnswer 对象，包含 Web 搜索 API 响应数据。
    /// </summary>
    [JsonPropertyName("webPages")]
    public ShaBingWebPages<T>? WebPages { get; set; }
}

/// <summary>
/// Sha Bing 用于请求的查询字符串。
/// </summary>
internal sealed class ShaBingQueryContext
{
    /// <summary>
    /// 请求中指定的查询字符串。
    /// </summary>
    [JsonPropertyName("originalQuery")]
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>
    /// Sha Bing 用于执行查询的查询字符串。如果原始查询字符串包含拼写错误，Sha Bing 会使用修改后的查询字符串。
    /// 例如，如果查询字符串是 saling downwind，修改后的查询字符串是 sailing downwind。
    /// </summary>
    /// <remarks>
    /// 仅当原始查询字符串包含拼写错误时，对象才包含此字段。
    /// </remarks>
    [JsonPropertyName("alteredQuery")]
    public string? AlteredQuery { get; set; }
}

/// <summary>
/// 与搜索查询相关的网页列表。
/// </summary>
#pragma warning disable CA1056 // 无法按照此类要求定义常量 Uri
internal sealed class ShaBingWebPages<T>
{
    /// <summary>
    /// 唯一标识网页搜索结果的 ID。
    /// </summary>
    /// <remarks>
    /// 仅当排名结果建议将所有网页搜索结果分组显示时，对象才包含此字段。有关如何使用此 ID 的更多信息，请参阅排名结果。
    /// </remarks>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 与查询相关的估计网页数量。使用此数字以及 count 和 offset 查询参数对结果进行分页。
    /// </summary>
    [JsonPropertyName("totalEstimatedMatches")]
    public long TotalEstimatedMatches { get; set; }

    /// <summary>
    /// 指向所请求网页的 Bing 搜索结果的 URL。
    /// </summary>
    [JsonPropertyName("webSearchUrl")]
    public string WebSearchUrl { get; set; } = string.Empty;

    /// <summary>
    /// 与查询相关的网页列表。
    /// </summary>
    [JsonPropertyName("value")]
    public IList<T>? Value { get; set; }
}
#pragma warning restore CA1056
#pragma warning restore CA1812



/// <summary>
/// 定义与查询相关的网页。
/// </summary>
public sealed class ShaBingWebPage
{
    /// <summary>
    /// 仅允许在本包内创建。
    /// </summary>
    [JsonConstructor]
    internal ShaBingWebPage() { }

    /// <summary>
    /// Bing 最后一次爬取该网页的时间。
    /// </summary>
    /// <remarks>
    /// 日期格式为 YYYY-MM-DDTHH:MM:SS。例如，2015-04-13T05:23:39。
    /// </remarks>
    [JsonPropertyName("dateLastCrawled")]
    public string? DateLastCrawled { get; set; }

    /// <summary>
    /// 网页的显示 URL。
    /// </summary>
    /// <remarks>
    /// 此 URL 仅用于显示目的，格式可能不正确。
    /// </remarks>
    [JsonPropertyName("displayUrl")]
#pragma warning disable CA1056 // 类似 URI 的属性不应为字符串
    public string? DisplayUrl { get; set; }
#pragma warning restore CA1056 // 类似 URI 的属性不应为字符串

    /// <summary>
    /// 此网页在网页搜索结果列表中的唯一标识符。
    /// </summary>
    /// <remarks>
    /// 仅当 Ranking 答案指定要将网页与其他搜索结果混合时，对象才包含此字段。
    /// 每个网页都包含一个与 Ranking 答案中的 ID 匹配的 ID。有关更多信息，请参阅排名结果。
    /// </remarks>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 网页的名称。
    /// </summary>
    /// <remarks>
    /// 将此名称与 url 一起使用，创建一个超链接，用户点击该链接即可访问该网页。
    /// </remarks>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 描述网页内容的网页文本片段。
    /// </summary>
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    /// <summary>
    /// 网页的 URL。
    /// </summary>
    /// <remarks>
    /// 将此 URL 与 name 一起使用，创建一个超链接，用户点击该链接即可访问该网页。
    /// </remarks>
    [JsonPropertyName("url")]
#pragma warning disable CA1056 // 类似 URI 的属性不应为字符串
    public string? Url { get; set; }
#pragma warning restore CA1056 // 类似 URI 的属性不应为字符串
}

```































