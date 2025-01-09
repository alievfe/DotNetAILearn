using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

public class Step6_Responsible_AI
{
    /// <summary>
    /// 展示如何使用提示过滤器来确保提示以负责任的方式呈现。
    /// </summary>
    public async Task AddPromptFilterAsync()
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

        // 向内核添加提示过滤器
        builder.Services.AddSingleton<IPromptRenderFilter, PromptFilter>();

        var kernel = builder.Build();

        KernelArguments arguments = new() { { "card_number", "4444 3333 2222 1111" } };

        var result = await kernel.InvokePromptAsync(
            "请告诉我关于这个信用卡号 {{$card_number}} 的一些有用信息？",
            arguments
        );

        Console.WriteLine(result);

        // 输出：对不起，但我无法提供帮助。
    }
}

internal sealed class PromptFilter : IPromptRenderFilter
{
    /// <summary>
    /// 在提示渲染之前异步调用的方法。
    /// </summary>
    /// <param name="context">包含提示渲染细节的 <see cref="PromptRenderContext"/> 实例。</param>
    /// <param name="next">指向管道中下一个过滤器或提示渲染操作本身的委托。如果不调用它，则不会调用下一个过滤器或提示渲染。</param>
    public async Task OnPromptRenderAsync(
        PromptRenderContext context,
        Func<PromptRenderContext, Task> next
    )
    {
        if (context.Arguments.ContainsName("card_number"))
        {
            context.Arguments["card_number"] = "**** **** **** ****";
        }
        await next(context);
        context.RenderedPrompt += " NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY";
        System.Console.WriteLine(context.RenderedPrompt);
    }
}
