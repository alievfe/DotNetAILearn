using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKUtils;
using SKUtils.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

/// <summary>
/// 此示例展示了如何创建和使用 <see cref="Kernel"/>。
/// </summary>
public sealed class Step1_Create_Kernel(ITestOutputHelper output) : BaseTest(output)
{
    /// <summary>
    /// 展示如何创建 <see cref="Kernel"/> 并使用它来执行提示。
    /// </summary>
    [Fact]
    public async Task CreateKernelAsync()
    {
        // 使用 OpenAI 聊天补全创建一个内核
        var kernel = ConfigExtensions.GetKernel("DouBao");

        // 示例 1. 使用提示调用内核并显示结果
        Console.WriteLine(await kernel.InvokePromptAsync("天空是什么颜色的？"));
        Console.WriteLine();

        // 示例 2. 使用模板化提示调用内核并显示结果
        KernelArguments arguments = new() { { "topic", "海洋" } };
        Console.WriteLine(await kernel.InvokePromptAsync("{{$topic}} 是什么颜色的？", arguments));
        Console.WriteLine();

        // 示例 3. 使用模板化提示调用内核并将结果流式传输到显示
        await foreach (
            var update in kernel.InvokePromptStreamingAsync("{{$topic}} 是什么颜色的？请提供详细解释。", arguments)
        )
        {
            Console.Write(update);
        }

        Console.WriteLine(string.Empty);

        // 示例 4. 使用模板化提示和执行设置调用内核
        arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0.5 })
        {
            { "topic", "狗" },
        };
        Console.WriteLine(await kernel.InvokePromptAsync("给我讲一个关于 {{$topic}} 的故事", arguments));

        // 示例 5. 使用模板化提示和配置为返回 JSON 的执行设置调用内核
        arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" })
        {
            { "topic", "巧克力" },
        };
        Console.WriteLine(
            await kernel.InvokePromptAsync("以 JSON 格式创建一个 {{$topic}} 蛋糕的食谱", arguments)
        );
    }
}
