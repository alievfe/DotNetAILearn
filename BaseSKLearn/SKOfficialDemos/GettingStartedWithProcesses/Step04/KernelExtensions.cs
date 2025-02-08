using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.History;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04;

/// <summary>
/// 为基于代理的流程模式提供便捷扩展方法。
/// </summary>
internal static class KernelExtensions
{
    /// <summary>
    /// 从单例的 <see cref="IChatHistoryProvider"/> 中获取聊天历史记录。
    /// </summary>
    public static IChatHistoryProvider GetHistory(this Kernel kernel) =>
        kernel.Services.GetRequiredService<IChatHistoryProvider>();

    /// <summary>
    /// 通过键来访问作为键控服务的代理。
    /// </summary>
    public static TAgent GetAgent<TAgent>(this Kernel kernel, string key)
        where TAgent : KernelAgent => kernel.Services.GetRequiredKeyedService<TAgent>(key);

    /// <summary>
    /// 使用作为键控服务访问的缩减器来总结聊天历史记录。
    /// </summary>
    public static async Task<string> SummarizeHistoryAsync(
        this Kernel kernel,
        string key,
        IReadOnlyList<ChatMessageContent> history
    )
    {
        ChatHistorySummarizationReducer reducer =
            kernel.Services.GetRequiredKeyedService<ChatHistorySummarizationReducer>(key);
        IEnumerable<ChatMessageContent>? reducedResponse = await reducer.ReduceAsync(history);
        ChatMessageContent summary =
            reducedResponse?.First() ?? throw new InvalidDataException("没有可用的总结内容");
        return summary.ToString();
    }
}
