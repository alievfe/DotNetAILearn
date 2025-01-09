using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SKUtils;
using SKUtils.SKExtensions;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

public class Step8_Pipelining
{
    /// <summary>
    /// 提供了一个示例，演示如何将多个函数组合成一个单一的函数，
    /// 该函数按顺序调用它们，将一个函数的输出作为下一个函数的输入传递。
    /// </summary>
    public async Task CreateFunctionPipelineAsync()
    {
        var configRoot = ConfigExtensions.LoadConfigFromJson();
        var chatConfig = configRoot.GetSection("DouBao").Get<OpenAIConfig>();
        var builder = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: chatConfig.ModelId,
                apiKey: chatConfig.ApiKey,
                endpoint: chatConfig.Endpoint
            );
        builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
        Kernel kernel = builder.Build();

        Console.WriteLine("================ PIPELINE ================");
        {
            // 创建一个函数管道，它会解析字符串为双精度浮点数，乘以另一个双精度浮点数，截断结果到整数，然后将其转换为人类可读的格式。
            KernelFunction parseDouble = KernelFunctionFactory.CreateFromMethod(
                (string s) => double.Parse(s, CultureInfo.InvariantCulture),
                "parseDouble"
            );
            KernelFunction multiplyByN = KernelFunctionFactory.CreateFromMethod(
                (double i, double n) => i * n,
                "multiplyByN"
            );
            KernelFunction truncate = KernelFunctionFactory.CreateFromMethod(
                (double d) => (int)d,
                "truncate"
            );
            KernelFunction humanize = KernelFunctionFactory.CreateFromPrompt(
                new PromptTemplateConfig()
                {
                    Template = "Spell out this number in English: {{$number}}",
                    InputVariables = [new() { Name = "number" }],
                }
            );
            KernelFunction pipeline = KernelFunctionCombinators.Pipe(
                [parseDouble, multiplyByN, truncate, humanize],
                "pipeline"
            );

            KernelArguments args = new() { ["s"] = "123.456", ["n"] = (double)78.90 };

            // - parseDouble 函数将被调用，从参数中读取 "123.456" 并解析为 (double)123.456。
            // - multiplyByN 函数将被调用，使用 i=123.456 和 n=78.90，并返回 (double)9740.6784。
            // - truncate 函数将被调用，使用 d=9740.6784，并返回 (int)9740，这将是最终结果。
            Console.WriteLine(await pipeline.InvokeAsync(kernel, args));
        }

        Console.WriteLine("================ GRAPH ================");
        {
            KernelFunction rand = KernelFunctionFactory.CreateFromMethod(
                () => Random.Shared.Next(),
                "GetRandomInt32"
            );
            KernelFunction mult = KernelFunctionFactory.CreateFromMethod(
                (int i, int j) => i * j,
                "Multiply"
            );
            KernelFunction graph = KernelFunctionCombinators.Pipe(
                [(rand, "i"), (rand, "j"), (mult, "")],
                "graph"
            );
            Console.WriteLine(await graph.InvokeAsync(kernel));
        }
    }
}
