using Microsoft.SemanticKernel.ChatCompletion;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04;

/// <summary>
/// 通过提供者访问聊天历史记录。
/// </summary>
/// <remarks>
/// 虽然基于内存的实现很简单，但这种抽象展示了在分布式服务中，如何从远程存储访问聊天历史记录。
/// <code>
/// class CosmosDbChatHistoryProvider(CosmosClient client, string sessionId) : IChatHistoryProvider { }
/// </code>
/// </remarks>
internal interface IChatHistoryProvider
{
    /// <summary>
    /// 提供对聊天历史记录的访问。
    /// </summary>
    Task<ChatHistory> GetHistoryAsync();

    /// <summary>
    /// 提交对聊天历史记录所做的任何更新。
    /// </summary>
    Task CommitAsync();
}

/// <summary>
/// <see cref="IChatHistoryProvider"/> 基于内存的具体实现。
/// </summary>
internal sealed class ChatHistoryProvider(ChatHistory history) : IChatHistoryProvider
{
    /// <inheritdoc/>
    public Task<ChatHistory> GetHistoryAsync() => Task.FromResult(history);

    /// <inheritdoc/>
    public Task CommitAsync()
    {
        return Task.CompletedTask;
    }
}
