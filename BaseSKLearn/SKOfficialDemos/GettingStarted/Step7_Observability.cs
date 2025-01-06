using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

public class Step7_Observability
{
    public async Task ObservabilityWithFiltersAsync()
    {
        // 使用 OpenAI 的聊天完成功能创建内核
        var configRoot = ConfigExtensions.LoadConfigFromJson();
        var chatConfig = configRoot.GetSection("DouBao").Get<OpenAIConfig>();
        var builder = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: chatConfig.ModelId,
                apiKey: chatConfig.ApiKey,
                endpoint: chatConfig.Endpoint
            );
        builder.Plugins.AddFromType<TimeInformation>();
        // 使用依赖注入添加过滤器
        builder.Services.AddSingleton<IFunctionInvocationFilter, MyFunctionFilter>();

        Kernel kernel = builder.Build();

        // 不使用依赖注入添加过滤器
        kernel.PromptRenderFilters.Add(new MyPromptFilter());

        // 使用提示调用内核，并允许AI自动调用函数
        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };
        Console.WriteLine(
            await kernel.InvokePromptAsync(
                "距离圣诞节还有多少天？请简单解释你的思考过程。",
                new(settings)
            )
        );
    }
    /// <summary>
    /// 返回当前时间的插件。
    /// </summary>
    private sealed class TimeInformation
    {
        [KernelFunction]
        [Description("获取当前UTC时间。")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 用于可观测性的函数调用过滤器。
    /// </summary>
    private sealed class MyFunctionFilter : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next
        )
        {
            System.Console.WriteLine($"Invoking {context.Function.Name}");
            await next(context);

            var metadata = context.Result.Metadata;
            if (metadata is not null && metadata.ContainsKey("Usage"))
            {
                System.Console.WriteLine($"Token usage: {metadata["Usage"]?.AsJson()}");
            }
        }
    }

    /// <summary>
    /// 用于可观测性的提示过滤器。
    /// </summary>
    private sealed class MyPromptFilter : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(
            PromptRenderContext context,
            Func<PromptRenderContext, Task> next
        )
        {
            System.Console.WriteLine($"Rendering prompt for {context.Function.Name}");
            await next(context);
            System.Console.WriteLine($"Rendered prompt: {context.RenderedPrompt}");
        }
    }
}
