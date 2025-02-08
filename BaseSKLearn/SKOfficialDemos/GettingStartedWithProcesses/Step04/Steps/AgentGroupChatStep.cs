using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Steps;

/// <summary>
/// 此步骤定义了群组聊天的操作，在该群组聊天中，多个代理协作以响应主代理的输入。
/// </summary>
public class AgentGroupChatStep : KernelProcessStep
{
    // 群组聊天服务的键，用于从服务容器中获取对应的 AgentGroupChat 实例
    public const string ChatServiceKey = $"{nameof(AgentGroupChatStep)}:{nameof(ChatServiceKey)}";

    // 缩减器服务的键，用于从服务容器中获取对应的 ChatHistorySummarizationReducer 实例
    public const string ReducerServiceKey =
        $"{nameof(AgentGroupChatStep)}:{nameof(ReducerServiceKey)}";

    public static class Functions
    {
        // 调用群组代理的函数名称
        public const string InvokeAgentGroup = nameof(InvokeAgentGroup);
    }

    /// <summary>
    /// 调用群组代理以处理输入并进行协作交互的异步方法。
    /// </summary>
    /// <param name="context">内核流程步骤上下文，用于发出事件。</param>
    /// <param name="kernel">语义内核实例，用于获取服务和执行总结操作。</param>
    /// <param name="input">主代理提供的输入信息。</param>
    [KernelFunction(Functions.InvokeAgentGroup)]
    public async Task InvokeAgentGroupAsync(
        KernelProcessStepContext context,
        Kernel kernel,
        string input
    )
    {
        // 从内核的服务容器中获取 AgentGroupChat 实例
        AgentGroupChat chat = kernel.GetRequiredService<AgentGroupChat>();

        // 重置聊天状态，清除之前调用留下的状态信息
        //await chat.ResetAsync();
        chat.IsComplete = false;

        // 创建一个表示用户输入的聊天消息内容对象
        ChatMessageContent message = new(AuthorRole.User, input);
        // 将用户输入的消息添加到群组聊天中
        chat.AddChatMessage(message);
        // 发出一个事件，表明群组接收到了消息
        await context.EmitEventAsync(
            new() { Id = AgentOrchestrationEvents.GroupMessage, Data = message }
        );

        // 异步迭代群组聊天的响应消息
        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
            // 针对每个响应消息发出事件，通知外部该群组有新消息
            await context.EmitEventAsync(
                new() { Id = AgentOrchestrationEvents.GroupMessage, Data = response }
            );
        }

        // 获取群组聊天的消息历史记录，并将其反转存储到数组中
        ChatMessageContent[] history = await chat.GetChatMessagesAsync().Reverse().ToArrayAsync();

        // 对群组聊天的历史记录进行总结，以便作为响应返回给主代理
        string summary = await kernel.SummarizeHistoryAsync(ReducerServiceKey, history);

        // 发出一个事件，表明群组聊天已经完成，并将总结信息作为数据传递
        await context.EmitEventAsync(
            new() { Id = AgentOrchestrationEvents.GroupCompleted, Data = summary }
        );
    }
}
