using System;
using System.Text.Json;

namespace SKUtils;

/// <summary>
/// 支持从可能包含字面量分隔符的文本块中解析 JSON：
/// <list type="table">
/// <item>
/// <code>
/// [json]
/// </code>
/// </item>
/// <item>
/// <code>
/// ```
/// [json]
/// ```
/// </code>
/// </item>
/// <item>
/// <code>
/// ```json
/// [json]
/// ```
/// </code>
/// </item>
/// </list>
/// </summary>
/// <remarks>
/// 在代理场景中，遇到这种形式的分隔符的 JSON 并不罕见。
/// </remarks>
public class JsonResultTranslator
{
    private const string LiteralDelimiter = "```";
    private const string JsonPrefix = "json";

    /// <summary>
    /// 用于从代理响应中提取 JSON 结果的实用方法。
    /// </summary>
    /// <param name="result">一个文本结果</param>
    /// <typeparam name="TResult">目标类型 <see cref="FunctionResult"/>。</typeparam>
    /// <returns>将 JSON 转换为请求的类型。</returns>
    public static TResult? Translate<TResult>(string? result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            return default;
        }
        string rawJson = ExtractJson(result);

        return JsonSerializer.Deserialize<TResult>(rawJson);
    }

    private static string ExtractJson(string result)
    {
        // 搜索初始的字面量分隔符：```
        int startIndex = result.IndexOf(LiteralDelimiter, StringComparison.Ordinal);
        // 没有初始分隔符，返回整个表达式。
        if (startIndex < 0)
            return result;
        startIndex += LiteralDelimiter.Length;

        // 处理 "json" 前缀（如果存在）。
        if (
            JsonPrefix.Equals(
                result.Substring(startIndex, JsonPrefix.Length),
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            startIndex += JsonPrefix.Length;
        }
        // 定位最终的字面量分隔符
        int endIndex = result.IndexOf(
            LiteralDelimiter,
            startIndex,
            StringComparison.OrdinalIgnoreCase
        );
        if (endIndex < 0)
        {
            endIndex = result.Length;
        }

        // 提取 JSON
        return result.Substring(startIndex, endIndex - startIndex);
    }
}
