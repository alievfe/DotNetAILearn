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



```cs
// 版权所有 (c) 微软公司。保留所有权利。

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;

namespace Microsoft.SemanticKernel;

/// <summary>
/// 用于向 <see cref="IServiceCollection"/> 注册 <see cref="ITextSearch"/> 的扩展方法。
/// </summary>
public static class WebServiceCollectionExtensions
{
    /// <summary>
    /// 使用指定的服务 ID 注册一个 <see cref="ITextSearch"/> 实例。
    /// </summary>
    /// <param name="services">要在其上注册 <see cref="ITextSearch"/> 的 <see cref="IServiceCollection"/>。</param>
    /// <param name="apiKey">用于对搜索服务的请求进行身份验证的 API 密钥凭证。</param>
    /// <param name="options">创建 <see cref="BingTextSearch"/> 时使用的 <see cref="BingTextSearchOptions"/> 实例。</param>
    /// <param name="serviceId">用作服务键的可选服务 ID。</param>
    public static IServiceCollection AddBingTextSearch(
        this IServiceCollection services,
        string apiKey,
        BingTextSearchOptions? options = null,
        string? serviceId = default)
    {
        // 向服务集合中添加键控单例服务
        services.AddKeyedSingleton<ITextSearch>(
            serviceId,
            (sp, obj) =>
            {
                // 如果传入的选项为空，则从服务提供者中获取 BingTextSearchOptions 实例
                var selectedOptions = options ?? sp.GetService<BingTextSearchOptions>();

                // 创建并返回 BingTextSearch 实例
                return new BingTextSearch(apiKey, selectedOptions);
            });

        return services;
    }

    /// <summary>
    /// 使用指定的服务 ID 注册一个 <see cref="ITextSearch"/> 实例。
    /// </summary>
    /// <param name="services">要在其上注册 <see cref="ITextSearch"/> 的 <see cref="IServiceCollection"/>。</param>
    /// <param name="searchEngineId">Google 搜索引擎 ID（格式类似 "a12b345..."）</param>
    /// <param name="apiKey">用于对搜索服务的请求进行身份验证的 API 密钥凭证。</param>
    /// <param name="options">创建 <see cref="GoogleTextSearch"/> 时使用的 <see cref="GoogleTextSearchOptions"/> 实例。</param>
    /// <param name="serviceId">用作服务键的可选服务 ID。</param>
    public static IServiceCollection AddGoogleTextSearch(
        this IServiceCollection services,
        string searchEngineId,
        string apiKey,
        GoogleTextSearchOptions? options = null,
        string? serviceId = default)
    {
        // 向服务集合中添加键控单例服务
        services.AddKeyedSingleton<ITextSearch>(
            serviceId,
            (sp, obj) =>
            {
                // 如果传入的选项为空，则从服务提供者中获取 GoogleTextSearchOptions 实例
                var selectedOptions = options ?? sp.GetService<GoogleTextSearchOptions>();

                // 创建并返回 GoogleTextSearch 实例
                return new GoogleTextSearch(searchEngineId, apiKey, selectedOptions);
            });

        return services;
    }
}
```



```cs
// 版权所有 (c) 微软公司。保留所有权利。

using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;

namespace Microsoft.SemanticKernel;

/// <summary>
/// 用于向 <see cref="IKernelBuilder"/> 注册 <see cref="ITextSearch"/> 的扩展方法。
/// </summary>
public static class WebKernelBuilderExtensions
{
    /// <summary>
    /// 使用指定的服务 ID 注册一个 <see cref="ITextSearch"/> 实例。
    /// </summary>
    /// <param name="builder">要在其上注册 <see cref="ITextSearch"/> 的 <see cref="IKernelBuilder"/>。</param>
    /// <param name="apiKey">用于对搜索服务的请求进行身份验证的 API 密钥凭证。</param>
    /// <param name="options">创建 <see cref="BingTextSearch"/> 时使用的 <see cref="BingTextSearchOptions"/> 实例。</param>
    /// <param name="serviceId">用作服务键的可选服务 ID。</param>
    public static IKernelBuilder AddBingTextSearch(
        this IKernelBuilder builder,
        string apiKey,
        BingTextSearchOptions? options = null,
        string? serviceId = default)
    {
        // 调用 WebServiceCollectionExtensions 中的 AddBingTextSearch 方法将 Bing 文本搜索服务添加到服务集合中
        builder.Services.AddBingTextSearch(apiKey, options, serviceId);

        return builder;
    }

    /// <summary>
    /// 使用指定的服务 ID 注册一个 <see cref="ITextSearch"/> 实例。
    /// </summary>
    /// <param name="builder">要在其上注册 <see cref="ITextSearch"/> 的 <see cref="IKernelBuilder"/>。</param>
    /// <param name="searchEngineId">Google 搜索引擎 ID（格式类似 "a12b345..."）</param>
    /// <param name="apiKey">用于对搜索服务的请求进行身份验证的 API 密钥凭证。</param>
    /// <param name="options">创建 <see cref="GoogleTextSearch"/> 时使用的 <see cref="GoogleTextSearchOptions"/> 实例。</param>
    /// <param name="serviceId">用作服务键的可选服务 ID。</param>
    public static IKernelBuilder AddGoogleTextSearch(
        this IKernelBuilder builder,
        string searchEngineId,
        string apiKey,
        GoogleTextSearchOptions? options = null,
        string? serviceId = default)
    {
        // 调用 WebServiceCollectionExtensions 中的 AddGoogleTextSearch 方法将 Google 文本搜索服务添加到服务集合中
        builder.Services.AddGoogleTextSearch(searchEngineId, apiKey, options, serviceId);
        return builder;
    }
}
```



```cs
// 版权所有 (c) 微软公司。保留所有权利。
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace GettingStartedWithTextSearch;

/// <summary>
/// 此示例展示了如何使用 <see cref="ITextSearch"/> 进行检索增强生成 (RAG)。
/// </summary>
public class Step2_Search_For_RAG(ITestOutputHelper output) : BaseTest(output)
{
    /// <summary>
    /// 展示如何从 <see cref="BingTextSearch"/> 创建默认的 <see cref="KernelPlugin"/>，并使用它为提示添加背景上下文。
    /// </summary>
    [Fact]
    public async Task RagWithTextSearchAsync()
    {
        // 创建一个使用 OpenAI 聊天完成的内核
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: TestConfiguration.OpenAI.ChatModelId,
            apiKey: TestConfiguration.OpenAI.ApiKey);
        Kernel kernel = kernelBuilder.Build();

        // 使用 Bing 搜索创建文本搜索实例
        ITextSearch textSearch = this.UseBingSearch ?
            new BingTextSearch(
                apiKey: TestConfiguration.Bing.ApiKey) :
            new GoogleTextSearch(
                searchEngineId: TestConfiguration.Google.SearchEngineId,
                apiKey: TestConfiguration.Google.ApiKey);

        // 构建一个带有网页搜索功能的文本搜索插件，并添加到内核中
        var searchPlugin = textSearch.CreateWithSearch("SearchPlugin");
        kernel.Plugins.Add(searchPlugin);

        // 调用提示，并使用文本搜索插件提供背景信息
        var query = "什么是 Semantic Kernel？";
        var prompt = "{{SearchPlugin.Search $query}}. {{$query}}";
        KernelArguments arguments = new() { { "query", query } };
        Console.WriteLine(await kernel.InvokePromptAsync(prompt, arguments));
    }

    /// <summary>
    /// 展示如何从 <see cref="ITextSearch"/> 创建默认的 <see cref="KernelPlugin"/>，并使用它为 Handlebars 提示添加背景上下文，同时在响应中包含引用信息。
    /// </summary>
    [Fact]
    public async Task RagWithBingTextSearchIncludingCitationsAsync()
    {
        // 创建一个使用 OpenAI 聊天完成的内核
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: TestConfiguration.OpenAI.ChatModelId,
            apiKey: TestConfiguration.OpenAI.ApiKey);
        Kernel kernel = kernelBuilder.Build();

        // 使用 Bing 搜索创建文本搜索实例
        var textSearch = new BingTextSearch(new(TestConfiguration.Bing.ApiKey));

        // 构建一个带有 Bing 搜索功能的文本搜索插件，并添加到内核中
        var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(searchPlugin);

        // 调用提示，并使用文本搜索插件提供背景信息
        var query = "什么是 Semantic Kernel？";
        string promptTemplate = """
            {{#with (SearchPlugin-GetTextSearchResults query)}}  
              {{#each this}}  
                名称: {{Name}}
                值: {{Value}}
                链接: {{Link}}
                -----------------
              {{/each}}  
            {{/with}}  

            {{query}}

            在响应中引用相关信息时，请包含引用出处。
            """;
        KernelArguments arguments = new() { { "query", query } };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();
        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        ));
    }

    /// <summary>
    /// 展示如何从 <see cref="ITextSearch"/> 创建默认的 <see cref="KernelPlugin"/>，并使用它为 Handlebars 提示添加背景上下文，同时在响应中包含引用信息和时间戳。
    /// </summary>
    [Fact]
    public async Task RagWithBingTextSearchIncludingTimeStampedCitationsAsync()
    {
        // 创建一个使用 OpenAI 聊天完成的内核
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: TestConfiguration.OpenAI.ChatModelId,
            apiKey: TestConfiguration.OpenAI.ApiKey);
        Kernel kernel = kernelBuilder.Build();

        // 使用 Bing 搜索创建文本搜索实例
        var textSearch = new BingTextSearch(new(TestConfiguration.Bing.ApiKey));

        // 构建一个带有 Bing 搜索功能的文本搜索插件，并添加到内核中
        var searchPlugin = textSearch.CreateWithGetSearchResults("SearchPlugin");
        kernel.Plugins.Add(searchPlugin);

        // 调用提示，并使用文本搜索插件提供背景信息
        var query = "什么是 Semantic Kernel？";
        string promptTemplate = """
            {{#with (SearchPlugin-GetSearchResults query)}}  
              {{#each this}}  
                名称: {{Name}}
                摘要: {{Snippet}}
                链接: {{DisplayUrl}}
                最后爬取日期: {{DateLastCrawled}}
                -----------------
              {{/each}}  
            {{/with}}  

            {{query}}

            在响应中引用相关信息时，请包含引用出处和信息日期。
            """;
        KernelArguments arguments = new() { { "query", query } };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();
        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        ));
    }

    /// <summary>
    /// 展示如何从 <see cref="ITextSearch"/> 创建默认的 <see cref="KernelPlugin"/>，并使用它为 Handlebars 提示添加背景上下文，该提示包含来自 Microsoft 开发者博客网站的搜索结果。
    /// </summary>
    [Fact]
    public async Task RagWithBingTextSearchUsingDevBlogsSiteAsync()
    {
        // 创建一个使用 OpenAI 聊天完成的内核
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: TestConfiguration.OpenAI.ChatModelId,
            apiKey: TestConfiguration.OpenAI.ApiKey);
        Kernel kernel = kernelBuilder.Build();

        // 使用 Bing 搜索创建文本搜索实例
        var textSearch = new BingTextSearch(new(TestConfiguration.Bing.ApiKey));

        // 创建一个过滤器，仅搜索 Microsoft 开发者博客网站
        var filter = new TextSearchFilter().Equality("site", "devblogs.microsoft.com");
        var searchOptions = new TextSearchOptions() { Filter = filter };

        // 构建一个带有 Bing 搜索功能的文本搜索插件，并添加到内核中
        var searchPlugin = KernelPluginFactory.CreateFromFunctions(
            "SearchPlugin", "仅搜索 Microsoft 开发者博客网站",
            [textSearch.CreateGetTextSearchResults(searchOptions: searchOptions)]);
        kernel.Plugins.Add(searchPlugin);

        // 调用提示，并使用文本搜索插件提供背景信息
        var query = "什么是 Semantic Kernel？";
        string promptTemplate = """
            {{#with (SearchPlugin-GetTextSearchResults query)}}  
              {{#each this}}  
                名称: {{Name}}
                值: {{Value}}
                链接: {{Link}}
                -----------------
              {{/each}}  
            {{/with}}  

            {{query}}

            在响应中引用相关信息时，请包含引用出处。
            """;
        KernelArguments arguments = new() { { "query", query } };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();
        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        ));
    }

    /// <summary>
    /// 展示如何从 <see cref="ITextSearch"/> 创建默认的 <see cref="KernelPlugin"/>，并使用它为 Handlebars 提示添加背景上下文，该提示包含指定网站的搜索结果。
    /// </summary>
    [Fact]
    public async Task RagWithBingTextSearchUsingCustomSiteAsync()
    {
        // 创建一个使用 OpenAI 聊天完成的内核
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: TestConfiguration.OpenAI.ChatModelId,
            apiKey: TestConfiguration.OpenAI.ApiKey);
        Kernel kernel = kernelBuilder.Build();

        // 使用 Bing 搜索创建文本搜索实例
        var textSearch = new BingTextSearch(new(TestConfiguration.Bing.ApiKey));

        // 构建一个带有 Bing 搜索功能的文本搜索插件，并添加到内核中
        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = "GetSiteResults",
            Description = "执行与指定查询相关的内容搜索，可选择仅搜索指定域名下的内容。",
            Parameters =
            [
                new KernelParameterMetadata("query") { Description = "要搜索的内容", IsRequired = true },
                new KernelParameterMetadata("top") { Description = "结果数量", IsRequired = false, DefaultValue = 5 },
                new KernelParameterMetadata("skip") { Description = "要跳过的结果数量", IsRequired = false, DefaultValue = 0 },
                new KernelParameterMetadata("site") { Description = "仅返回此域名下的结果", IsRequired = false },
            ],
            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) },
        };
        var searchPlugin = KernelPluginFactory.CreateFromFunctions("SearchPlugin", "搜索指定网站", [textSearch.CreateGetTextSearchResults(options)]);
        kernel.Plugins.Add(searchPlugin);

        // 调用提示，并使用文本搜索插件提供背景信息
        var query = "什么是 Semantic Kernel？";
        string promptTemplate = """
            {{#with (SearchPlugin-GetSiteResults query)}}  
              {{#each this}}  
                名称: {{Name}}
                值: {{Value}}
                链接: {{Link}}
                -----------------
              {{/each}}  
            {{/with}}  

            {{query}}

            仅包含来自 techcommunity.microsoft.com 的结果。 
            在响应中引用相关信息时，请包含引用出处。
            """;
        KernelArguments arguments = new() { { "query", query } };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();
        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        ));
    }

    /// <summary>
    /// 展示如何从 <see cref="ITextSearch"/> 创建默认的 <see cref="KernelPlugin"/>，并使用它为 Handlebars 提示添加背景上下文，该提示包含完整的网页内容。
    /// </summary>
    [Fact]
    public async Task RagWithBingTextSearchUsingFullPagesAsync()
    {
        // 创建一个使用 OpenAI 聊天完成的内核
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: TestConfiguration.OpenAI.ChatModelId, // 需要大上下文窗口的模型，例如 gpt - 4o 或 gpt - 4o - mini 
            apiKey: TestConfiguration.OpenAI.ApiKey);
        Kernel kernel = kernelBuilder.Build();

        // 使用 Bing 搜索创建文本搜索实例
        var textSearch = new TextSearchWithFullValues(new BingTextSearch(new(TestConfiguration.Bing.ApiKey)));

        // 创建一个过滤器，仅搜索 Microsoft 开发者博客网站
        var filter = new TextSearchFilter().Equality("site", "devblogs.microsoft.com");
        var searchOptions = new TextSearchOptions() { Filter = filter };

        // 构建一个带有 Bing 搜索功能的文本搜索插件，并添加到内核中
        var searchPlugin = KernelPluginFactory.CreateFromFunctions(
            "SearchPlugin", "仅搜索 Microsoft 开发者博客网站",
            [textSearch.CreateGetTextSearchResults(searchOptions: searchOptions)]);
        kernel.Plugins.Add(searchPlugin);

        // 调用提示，并使用文本搜索插件提供背景信息
        var query = "什么是 Semantic Kernel？";
        string promptTemplate = """
            {{#with (SearchPlugin-GetTextSearchResults query)}}  
              {{#each this}}  
                名称: {{Name}}
                值: {{Value}}
                链接: {{Link}}
                -----------------
              {{/each}}  
            {{/with}}  

            {{query}}

            在响应中引用相关信息时，请包含引用出处。
            """;
        KernelArguments arguments = new() { { "query", query } };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();
        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        ));
    }
}

/// <summary>
/// 包装 <see cref="ITextSearch"/> 以提供完整的网页作为搜索结果。
/// </summary>
public partial class TextSearchWithFullValues(ITextSearch searchDelegate) : ITextSearch
{
    /// <inheritdoc/>
    public Task<KernelSearchResults<object>> GetSearchResultsAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        return searchDelegate.GetSearchResultsAsync(query, searchOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<KernelSearchResults<TextSearchResult>> GetTextSearchResultsAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        var results = await searchDelegate.GetTextSearchResultsAsync(query, searchOptions, cancellationToken);

        var resultList = new List<TextSearchResult>();

        using HttpClient client = new();
        await foreach (var item in results.Results.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            string? value = item.Value;
            try
            {
                if (item.Link is not null)
                {
                    value = await client.GetStringAsync(new Uri(item.Link), cancellationToken);
                    value = ConvertHtmlToPlainText(value);
                }
            }
            catch (HttpRequestException)
            {
            }

            resultList.Add(new(value) { Name = item.Name, Link = item.Link });
        }

        return new KernelSearchResults<TextSearchResult>(resultList.ToAsyncEnumerable<TextSearchResult>(), results.TotalCount, results.Metadata);
    }

    /// <inheritdoc/>
    public Task<KernelSearchResults<string>> SearchAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        return searchDelegate.SearchAsync(query, searchOptions, cancellationToken);
    }

    /// <summary>
    /// 将 HTML 转换为纯文本。
    /// </summary>
    private static string ConvertHtmlToPlainText(string html)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        string text = doc.DocumentNode.InnerText;
        text = MyRegex().Replace(text, " "); // 去除不必要的空白字符  
        return text.Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MyRegex();
}
```



resp

```
Hello, World!


----log------

----查询输入 我又幻想了是什么梗？

----查询条件 {"IncludeTotalCount":false,"Filter":null,"Top":2,"Skip":0}

----搜索内容 {
  "_type": "SearchResponse",
  "queryContext": {
    "originalQuery": "我又幻想了是什么梗？",
    "alteredQuery": null
  },
  "webPages": {
    "id": "c500db18-8090-49d6-985e-d9a75d321d49",
    "totalEstimatedMatches": 58800000,
    "webSearchUrl": "https://cn.bing.com/search?q=%E6%88%91%E5%8F%88%E5%B9%BB%E6%83%B3%E4%BA%86%E6%98%AF%E4%BB%80%E4%B9%88%E6%A2%97%EF%BC%9F",
    "value": [
      {
        "id": "1",
        "name": "我又幻想了是什么梗【瓦龙梗百科】_英雄联盟 - 哔哩哔哩",
        "displayUrl": "https://www.bilibili.com/video/BV15NAVeZEXL/",
        "url": "https://www.bilibili.com/video/BV15NAVeZEXL/",
        "snippet": "简介：我又幻想了是什么梗 引用素材： BV17AwneJERJ B；更多实用攻略教学，爆笑沙雕集锦，你所不知道的游戏知识，热门游戏视频7*24小时持续更新,尽在哔哩哔哩bilibili 视频 …",
        "siteName": "哔哩哔哩",
        "siteIcon": "https://ts4.cn.mm.bing.net/th?id=ODLS.1e67f861-d980-4c01-ba66-9450f90172f4&w=32&h=32&qlt=90&pcl=fffffa&o=6&pid=1.2",
        "siteImage": null,
        "dateLastCrawled": "2025-02-16"
      },
      {
        "id": "2",
        "name": "又幻想了是什么梗【梗指南】_哔哩哔哩_bilibili",
        "displayUrl": "https://www.bilibili.com/video/BV1QiAzeaES7/",
        "url": "https://www.bilibili.com/video/BV1QiAzeaES7/",
        "snippet": "又幻想了是什么梗引用素材：BV17AwneJERJBV1GcNdeEERZBV1ggA3e2EYjBV1yNNfeGEktBV1x4APeQEY9BV19aANeTEXzBV1iFAie8EXZ* …",
        "siteName": "哔哩哔哩",
        "siteIcon": "https://ts4.cn.mm.bing.net/th?id=ODLS.1e67f861-d980-4c01-ba66-9450f90172f4&w=32&h=32&qlt=91&pcl=fffffa&o=6&pid=1.2",
        "siteImage": null,
        "dateLastCrawled": "2025-02-20"
      }
    ]
  }
}

----log------

Request: POST https://ark.cn-beijing.volces.com/api/v3/chat/completions
Bearer Token: 7864919b-af93-4c85-9d31-92918a921c33
Request Body: {"messages":[{"role":"user","content":"System.Collections.Generic.List\u00601[System.String]. \u6211\u53C8\u5E7B\u60F3\u4E86\u662F\u4EC0\u4E48\u6897\uFF1F"}],"model":"ep-20250207134353-9wcjp"}
Response Body: {"choices":[{"finish_reason":"stop","index":0,"logprobs":null,"message":{"content":"“System.Collections.Generic.List`1[System.String]. 我又幻想了”是网络上一个比较魔性、无厘头的梗。\n\n### 来源\n它最初出自一段聊天记录。其中一 人回复的内容包含“System.Collections.Generic.List`1[System.String]. 我又幻想了”这样看似非常专业的计算机代码相关表述与日常幻想话语的奇怪组合，这种不搭调的拼接和其莫名的风格引发了网友的关注和兴趣。\n\n### 传播与特点\n之后在网络上广泛传播，大家会在一些搞笑、调侃或者想要营造无厘头氛围的场景中使用这句话，它以其独特的怪异感给人留下深刻印象，成为一种网络流行的搞笑表达。   ","role":"assistant"}}],"created":1740035952,"id":"021740035947232c2649323859df38f4624a67d95e98392dd1e8d","model":"doubao-1-5-pro-32k-250115","object":"chat.completion","usage":{"completion_tokens":158,"prompt_tokens":26,"total_tokens":184,"prompt_tokens_details":{"cached_tokens":0},"completion_tokens_details":{"reasoning_tokens":0}}}
“System.Collections.Generic.List`1[System.String]. 我又幻想了”是网络上一个比较魔性、无厘头的梗。

### 来源
它最初出自一段聊天记录。其中一人回复的内容包含“System.Collections.Generic.List`1[System.String]. 我又幻想了”这样看似非常专业的计算机代码相关表述与日常幻想话语的奇怪组合，这种不搭调的拼接和其莫名的风格引发了网友的关注和兴趣。

### 传播与特点
之后在网络上广泛传播，大家会在一些搞笑、调侃或者想要营造无厘头氛围的场景中使用这句话，它以其独特的怪异感给人留下深刻印象，成为一种网络流行的搞笑表达。

```



菊花茶是什么？



nmsl是什么网络用语

武夷岩茶是什么

太极是

