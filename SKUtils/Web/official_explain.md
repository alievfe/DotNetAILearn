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





```cs
// 版权所有 (c) 微软公司。保留所有权利。

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Plugins.Web.Bing;

/// <summary>
/// Bing API 连接器。
/// </summary>
public sealed class BingConnector : IWebSearchEngineConnector
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly Uri? _uri = null;
    private const string DefaultUri = "https://api.bing.microsoft.com/v7.0/search?q";

    /// <summary>
    /// 初始化 <see cref="BingConnector"/> 类的新实例。
    /// </summary>
    /// <param name="apiKey">用于验证连接器的 API 密钥。</param>
    /// <param name="uri">Bing 搜索实例的 URI。默认为 "https://api.bing.microsoft.com/v7.0/search?q"。</param>
    /// <param name="loggerFactory">用于日志记录的 <see cref="ILoggerFactory"/>。如果为 null，则不进行日志记录。</param>
    public BingConnector(string apiKey, Uri? uri = null, ILoggerFactory? loggerFactory = null) :
        this(apiKey, HttpClientProvider.GetHttpClient(), uri, loggerFactory)
    {
    }

    /// <summary>
    /// 初始化 <see cref="BingConnector"/> 类的新实例。
    /// </summary>
    /// <param name="apiKey">用于验证连接器的 API 密钥。</param>
    /// <param name="httpClient">用于发出请求的 HTTP 客户端。</param>
    /// <param name="uri">Bing 搜索实例的 URI。默认为 "https://api.bing.microsoft.com/v7.0/search?q"。</param>
    /// <param name="loggerFactory">用于日志记录的 <see cref="ILoggerFactory"/>。如果为 null，则不进行日志记录。</param>
    public BingConnector(string apiKey, HttpClient httpClient, Uri? uri = null, ILoggerFactory? loggerFactory = null)
    {
        // 验证 HTTP 客户端不为空
        Verify.NotNull(httpClient);

        this._apiKey = apiKey;
        // 创建日志记录器，如果未提供日志工厂，则使用空日志记录器
        this._logger = loggerFactory?.CreateLogger(typeof(BingConnector)) ?? NullLogger.Instance;
        this._httpClient = httpClient;
        // 添加用户代理请求头
        this._httpClient.DefaultRequestHeaders.Add("User-Agent", HttpHeaderConstant.Values.UserAgent);
        // 添加语义内核版本请求头
        this._httpClient.DefaultRequestHeaders.Add(HttpHeaderConstant.Names.SemanticKernelVersion, HttpHeaderConstant.Values.GetAssemblyVersion(typeof(BingConnector)));
        // 如果未提供 URI，则使用默认 URI
        this._uri = uri ?? new Uri(DefaultUri);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> SearchAsync<T>(string query, int count = 1, int offset = 0, CancellationToken cancellationToken = default)
    {
        // 检查 count 参数是否在有效范围内
        if (count is <= 0 or >= 50)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"{nameof(count)} 值必须大于 0 且小于 50。");
        }

        // 构建请求 URI
        Uri uri = new($"{this._uri}={Uri.EscapeDataString(query.Trim())}&count={count}&offset={offset}");

        // 记录发送的请求 URI
        this._logger.LogDebug("正在发送请求: {Uri}", uri);

        // 发送 GET 请求并获取响应
        using HttpResponseMessage response = await this.SendGetRequestAsync(uri, cancellationToken).ConfigureAwait(false);

        // 记录响应的状态码
        this._logger.LogDebug("收到响应: {StatusCode}", response.StatusCode);

        // 读取响应内容为字符串，并处理可能的异常
        string json = await response.Content.ReadAsStringWithExceptionMappingAsync(cancellationToken).ConfigureAwait(false);

        // 响应内容属于敏感数据，使用跟踪日志记录，默认情况下禁用
        this._logger.LogTrace("收到的响应内容: {Data}", json);

        // 将 JSON 字符串反序列化为 WebSearchResponse 对象
        WebSearchResponse? data = JsonSerializer.Deserialize<WebSearchResponse>(json);

        List<T>? returnValues = null;
        if (data?.WebPages?.Value is not null)
        {
            if (typeof(T) == typeof(string))
            {
                // 如果请求类型为 string，则提取网页摘要作为返回值
                WebPage[]? results = data?.WebPages?.Value;
                returnValues = results?.Select(x => x.Snippet).ToList() as List<T>;
            }
            else if (typeof(T) == typeof(WebPage))
            {
                // 如果请求类型为 WebPage，则返回网页列表
                List<WebPage>? webPages = [.. data.WebPages.Value];
                returnValues = webPages.Take(count).ToList() as List<T>;
            }
            else
            {
                // 如果请求类型不支持，则抛出异常
                throw new NotSupportedException($"类型 {typeof(T)} 不受支持。");
            }
        }

        return
            returnValues is null ? Array.Empty<T>() :
            returnValues.Count <= count ? returnValues :
            returnValues.Take(count);
    }

    /// <summary>
    /// 向指定的 URI 发送 GET 请求。
    /// </summary>
    /// <param name="uri">要发送请求的 URI。</param>
    /// <param name="cancellationToken">用于取消请求的取消令牌。</param>
    /// <returns>表示请求响应的 <see cref="HttpResponseMessage"/>。</returns>
    private async Task<HttpResponseMessage> SendGetRequestAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        // 创建一个 HTTP GET 请求消息
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

        // 如果 API 密钥不为空，则添加到请求头中
        if (!string.IsNullOrEmpty(this._apiKey))
        {
            httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", this._apiKey);
        }

        // 发送请求并检查响应是否成功
        return await this._httpClient.SendWithSuccessCheckAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }
}
```



```cs
/// <summary>
/// 网络搜索引擎连接器接口。
/// </summary>
public interface IWebSearchEngineConnector
{
    /// <summary>
    /// 执行一次网络搜索引擎搜索。
    /// </summary>
    /// <param name="query">要搜索的查询内容。</param>
    /// <param name="count">搜索结果的数量。</param>
    /// <param name="offset">要跳过的搜索结果数量。</param>
    /// <param name="cancellationToken">用于监视取消请求的 <see cref="CancellationToken"/>。默认值为 <see cref="CancellationToken.None"/>。</param>
    /// <returns>搜索返回的首批片段内容。</returns>
    Task<IEnumerable<T>> SearchAsync<T>(string query, int count = 1, int offset = 0, CancellationToken cancellationToken = default);
}
```



```cs
// 版权所有 (c) 微软公司。保留所有权利。

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Plugins.Web;

/// <summary>
/// 一个密封类，包含来自相应网络搜索 API 的反序列化响应。
/// </summary>
/// <returns>一个 WebPage 对象，包含网络搜索 API 的响应数据。</returns>
[SuppressMessage("Performance", "CA1056:更改参数 'uri' 的类型...",
    Justification = "无法按照此类要求定义常量 Uri")]
public sealed class WebPage
{
    /// <summary>
    /// 搜索结果的名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 搜索结果的 URL。
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 搜索结果的摘要。
    /// </summary>
    [JsonPropertyName("snippet")]
    public string Snippet { get; set; } = string.Empty;
}

/// <summary>
/// 一个密封类，包含来自相应网络搜索 API 的反序列化响应。
/// </summary>
/// <returns>一个 WebPages? 对象，包含搜索 API 响应数据中的 WebPages 数组，或者为 null。</returns>
public sealed class WebSearchResponse
{
    /// <summary>
    /// 一个可空的 WebPages 对象，包含网络搜索 API 的响应数据。
    /// </summary>
    [JsonPropertyName("webPages")]
    public WebPages? WebPages { get; set; }
}

/// <summary>
/// 一个密封类，包含来自相应网络搜索 API 的反序列化响应。
/// </summary>
/// <returns>一个 WebPages 数组对象，包含网络搜索 API 的响应数据。</returns>
[SuppressMessage("Performance", "CA1819:属性不应返回数组", Justification = "网络搜索 API 要求如此")]
public sealed class WebPages
{
    /// <summary>
    /// 一个可空的 WebPage 数组对象，包含网络搜索 API 的响应数据。
    /// </summary>
    [JsonPropertyName("value")]
    public WebPage[]? Value { get; set; }
}
```























