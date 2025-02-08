using System.ComponentModel;
using System.Text.Json;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ChatResponseFormat = OpenAI.Chat.ChatResponseFormat;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Steps;

/// <summary>
/// 此步骤为主要代理定义操作。该代理负责与用户进行交互，
/// 同时将任务委托给一组代理。
/// </summary>
public class ManagerAgentStep : KernelProcessStep
{
    public const string AgentServiceKey = $"{nameof(ManagerAgentStep)}:{nameof(AgentServiceKey)}";
    public const string ReducerServiceKey =
        $"{nameof(ManagerAgentStep)}:{nameof(ReducerServiceKey)}";

    public static class Functions
    {
        public const string InvokeAgent = nameof(InvokeAgent);
        public const string InvokeGroup = nameof(InvokeGroup);
        public const string ReceiveResponse = nameof(ReceiveResponse);
    }

    [KernelFunction(Functions.InvokeAgent)]
    public async Task InvokeAgentAsync(
        KernelProcessStepContext context,
        Kernel kernel,
        string userInput,
        ILogger logger
    )
    {
        // 获取聊天历史记录
        IChatHistoryProvider historyProvider = kernel.GetHistory();
        ChatHistory history = await historyProvider.GetHistoryAsync();

        // 将用户输入添加到聊天历史记录中
        history.Add(new ChatMessageContent(AuthorRole.User, userInput));

        // 获取代理的响应
        ChatCompletionAgent agent = kernel.GetAgent<ChatCompletionAgent>(AgentServiceKey);
        await foreach (ChatMessageContent message in agent.InvokeAsync(history))
        {
            // 记录每个响应
            history.Add(message);

            // 为每个代理响应发出事件渲染输出
            await context.EmitEventAsync(
                new() { Id = AgentOrchestrationEvents.AgentResponse, Data = message }
            );
        }

        // 提交对聊天历史记录所做的任何更改
        await historyProvider.CommitAsync();

        // 评估当前意图
        IntentResult intent = await IsRequestingUserInputAsync(kernel, history, logger);

        string intentEventId =
            intent.IsRequestingUserInput ? AgentOrchestrationEvents.AgentResponded
            : intent.IsWorking ? AgentOrchestrationEvents.AgentWorking
            : CommonEvents.UserInputComplete;

        await context.EmitEventAsync(new() { Id = intentEventId });
    }

    [KernelFunction(Functions.InvokeGroup)]
    public async Task InvokeGroupAsync(KernelProcessStepContext context, Kernel kernel)
    {
        // 获取聊天历史记录
        IChatHistoryProvider historyProvider = kernel.GetHistory();
        ChatHistory history = await historyProvider.GetHistoryAsync();

        // 总结与用户的对话，作为代理组的输入
        string summary = await kernel.SummarizeHistoryAsync(ReducerServiceKey, history);

        await context.EmitEventAsync(
            new() { Id = AgentOrchestrationEvents.GroupInput, Data = summary }
        );
    }

    [KernelFunction(Functions.ReceiveResponse)]
    public async Task ReceiveResponseAsync(
        KernelProcessStepContext context,
        Kernel kernel,
        string response
    )
    {
        // 获取聊天历史记录
        IChatHistoryProvider historyProvider = kernel.GetHistory();
        ChatHistory history = await historyProvider.GetHistoryAsync();

        // 转发内部响应
        ChatCompletionAgent agent = kernel.GetAgent<ChatCompletionAgent>(AgentServiceKey);
        ChatMessageContent message = new(AuthorRole.Assistant, response)
        {
            AuthorName = agent.Name,
        };
        history.Add(message);

        await context.EmitEventAsync(
            new() { Id = AgentOrchestrationEvents.AgentResponse, Data = message }
        );

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.AgentResponded });
    }

    private static async Task<IntentResult> IsRequestingUserInputAsync(
        Kernel kernel,
        ChatHistory history,
        ILogger logger
    )
    {
        ChatHistory localHistory =
        [
            new ChatMessageContent(AuthorRole.System, "分析对话并确定是否正在征求用户输入。"),
            .. history.TakeLast(1),
        ];

        IChatCompletionService service = kernel.GetRequiredService<IChatCompletionService>();

        // ResponseFormat 属性用于获取或设置完成结果的响应格式，可设为 "json_object" 或 "text" 字符串、ChatResponseFormat 对象、Type 对象（自动创建 JSON 架构），设为 { "type": "json_schema", "json_schema": { ...} } 启用结构化输出，设为 { "type": "json_object" } 启用 JSON 模式，但使用 JSON 模式需通过系统或用户消息指示模型生成 JSON，否则可能导致请求异常，且内容可能因超限制被截断。
        // 以下使用json_schema
        ChatMessageContent response = await service.GetChatMessageContentAsync(
            localHistory,
            new OpenAIPromptExecutionSettings { ResponseFormat = s_intentResponseFormat }
        );
        IntentResult intent = JsonSerializer.Deserialize<IntentResult>(response.ToString())!;

        logger.LogTrace(
            "{StepName} 响应意图 - {IsRequestingUserInput}: {Rationale}",
            nameof(ManagerAgentStep),
            intent.IsRequestingUserInput,
            intent.Rationale
        );

        return intent;
    }

    private static readonly ChatResponseFormat s_intentResponseFormat =
        ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "intent_result",
            jsonSchema: BinaryData.FromString(JsonSchemaGenerator.FromType<IntentResult>()),
            jsonSchemaIsStrict: true
        );

    [DisplayName("意图结果")]
    [Description("这是结果的描述")]
    public sealed record IntentResult(
        [property: Description(
            "如果请求或征求用户输入，则为 True。仅向用户讲话而无特定请求为 False。向用户提问为 True。"
        )]
            bool IsRequestingUserInput,
        [property: Description("如果正在处理用户请求，则为 True。")] bool IsWorking,
        [property: Description("分配给 IsRequestingUserInput 值的理由")] string Rationale
    );
}
