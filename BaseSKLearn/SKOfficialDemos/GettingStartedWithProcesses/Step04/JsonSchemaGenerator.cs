using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04;

/// <summary>
/// 该静态类用于生成 JSON 模式。
/// </summary>
internal static class JsonSchemaGenerator
{
    /// <summary>
    /// 从 .NET 类型生成 JSON 模式字符串的包装方法。
    /// </summary>
    /// <typeparam name="TSchemaType">要生成 JSON 模式的 .NET 类型。</typeparam>
    /// <returns>生成的 JSON 模式字符串。</returns>
    public static string FromType<TSchemaType>()
    {
        // 配置 JSON 序列化选项，不允许未映射的成员
        JsonSerializerOptions options = new(JsonSerializerOptions.Default)
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        // 配置 AI JSON 模式创建选项
        AIJsonSchemaCreateOptions config = new()
        {
            IncludeSchemaKeyword = false,
            DisallowAdditionalProperties = true,
        };

        // 构建 JSON 模式并将其转换为 JSON 字符串返回
        return KernelJsonSchemaBuilder.Build(typeof(TSchemaType), "意图结果", config).AsJson();
    }
}
