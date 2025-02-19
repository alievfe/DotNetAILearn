using System.Text.Json.Serialization;

namespace SKUtils.Web;

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
