using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils;
using SKUtils.SKExtensions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示解析JSON响应。
/// 测试DouBao不理解一定要输出Integer导致报错。DeepSeek可行。
/// </summary>
public class Step05_JsonResult
{
    // 分数阈值
    private const int ScoreCompletionThreshold = 70;

    // 导师
    private const string TutorName = "Tutor";

    /*
    逐步思考，并从创造力和表达力方面对用户输入进行评分，评分范围为1-100。
    以JSON格式响应，遵循以下JSON模式：
    {
        "score": "整数 (1-100)",
        "notes": "评分的原因"
    }
    */
    private const string TutorInstructions = """
        Think step-by-step and rate the user input on creativity and expressiveness from 1-100.

        Respond in JSON format with the following JSON schema:

        {
            "score": "integer (1-100)",
            "notes": "the reason for your score"
        }
        """;

    public async Task UseKernelFunctionStrategiesWithJsonResultAsync()
    {
        // 定义代理
        ChatCompletionAgent agent = new()
        {
            Instructions = TutorInstructions,
            Name = TutorName,
            Kernel = ConfigExtensions.GetKernel("DeepSeek"),
        };
        // 创建一个用于代理交互的聊天。
        AgentGroupChat chat = new()
        {
            ExecutionSettings = new()
            {
                // 这里使用了一个TerminationStrategy子类，当响应中的分数大于或等于70时，将终止聊天。
                TerminationStrategy = new ThresholdTerminationStrategy(),
            },
        };

        // 响应用户输入
        await InvokeAgentAsync("The sunset is very colorful.");
        await InvokeAgentAsync("The sunset is setting over the mountains.");
        await InvokeAgentAsync(
            "The sunset is setting over the mountains and filled the sky with a deep red flame, setting the clouds ablaze."
        );

        // 本地函数，用于调用代理并显示聊天消息。
        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.AddChatMessage(message);
            message.WriteAgentChatMessage();

            await foreach (ChatMessageContent response in chat.InvokeAsync(agent))
            {
                response.WriteAgentChatMessage();
                Console.WriteLine($"[IS COMPLETED: {chat.IsComplete}]");
            }
        }
    }

    private sealed class ThresholdTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(
            Agent agent,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken
        )
        {
            string lastMessageContent = history[^1].Content ?? string.Empty;
            WritingScore? result = JsonResultTranslator.Translate<WritingScore>(lastMessageContent);
            return Task.FromResult((result?.score ?? 0) >= ScoreCompletionThreshold);
        }
    }

    private record struct WritingScore(int score, string notes);
}
