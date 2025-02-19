using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace SKUtils.Web;

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
