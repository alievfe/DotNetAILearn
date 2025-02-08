using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04;

// 待办事项：JSON 模式应与实际执行序列化时使用的 JsonSerializerOptions 相匹配，
// 例如，模式中是否应包含公共字段应与公共字段是否会被序列化/反序列化相匹配。
// 目前我们可以采用默认设置，但如果能够通过内核提供 JsonSerializerOptions，我们应该：
// 1) 在构建模式时使用创建 KernelFunction 时所用内核的 JsonSerializerOptions
// 2) 在使用模式时（例如函数调用）检查所用的 JsonSerializerOptions 是否与构建模式时所用的相匹配，
//    如果不匹配，则为该 JsonSerializerOptions 生成新的模式
internal static class KernelJsonSchemaBuilder
{
    private static JsonSerializerOptions? s_options;
    internal static readonly AIJsonSchemaCreateOptions s_schemaOptions = new()
    {
        // 不包含 "$schema" 关键字
        IncludeSchemaKeyword = false,
        // 在枚举模式中包含类型信息
        IncludeTypeInEnumSchemas = true,
        // 不要求所有属性都存在
        RequireAllProperties = false,
        // 允许额外的属性
        DisallowAdditionalProperties = false,
    };

    // 表示始终匹配的 JSON 模式（空对象）
    private static readonly JsonElement s_trueSchemaAsObject = JsonDocument.Parse("{}").RootElement;

    // 表示始终不匹配的 JSON 模式
    private static readonly JsonElement s_falseSchemaAsObject = JsonDocument
        .Parse("""{"not":true}""")
        .RootElement;

    [RequiresUnreferencedCode("使用反射来生成 JSON 模式，因此与 AOT 场景不兼容。")]
    [RequiresDynamicCode("使用反射来生成 JSON 模式，因此与 AOT 场景不兼容。")]
    public static KernelJsonSchema Build(
        Type type,
        string? description = null,
        AIJsonSchemaCreateOptions? configuration = null
    )
    {
        return Build(type, GetDefaultOptions(), description, configuration);
    }

    public static KernelJsonSchema Build(
        Type type,
        JsonSerializerOptions options,
        string? description = null,
        AIJsonSchemaCreateOptions? configuration = null
    )
    {
        configuration ??= s_schemaOptions;
        // 根据指定类型、描述、序列化选项和推理选项创建 JSON 模式
        JsonElement schemaDocument = AIJsonUtilities.CreateJsonSchema(
            type,
            description,
            serializerOptions: options,
            inferenceOptions: configuration
        );
        switch (schemaDocument.ValueKind)
        {
            case JsonValueKind.False:
                schemaDocument = s_falseSchemaAsObject;
                break;
            case JsonValueKind.True:
                schemaDocument = s_trueSchemaAsObject;
                break;
        }

        return KernelJsonSchema.Parse(schemaDocument.GetRawText());
    }

    [RequiresUnreferencedCode(
        "使用了 JsonStringEnumConverter 和 DefaultJsonTypeInfoResolver 类，因此与 AOT 场景不兼容。"
    )]
    [RequiresDynamicCode(
        "使用了 JsonStringEnumConverter 和 DefaultJsonTypeInfoResolver 类，因此与 AOT 场景不兼容。"
    )]
    private static JsonSerializerOptions GetDefaultOptions()
    {
        if (s_options is null)
        {
            JsonSerializerOptions options = new()
            {
                // 使用默认的类型信息解析器
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                // 添加枚举字符串转换器
                Converters = { new JsonStringEnumConverter() },
            };
            // 使选项只读
            options.MakeReadOnly();
            s_options = options;
        }

        return s_options;
    }
}
