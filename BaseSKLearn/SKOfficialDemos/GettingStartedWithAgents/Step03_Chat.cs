using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils;
using SKUtils.SKExtensions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示如何使用 <see cref="AgentGroupChatSettings"/> 创建 <see cref="AgentChat"/>，
/// 这些设置决定了聊天如何进行，包括：代理选择、聊天继续以及最大代理交互次数。
/// </summary>
public class Step03_Chat
{
    private const string ReviewerName = "ArtDirector";
    private const string ReviewerInstructions = """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine if the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without example.
        """;

    private const string CopyWriterName = "CopyWriter";
    private const string CopyWriterInstructions = """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """;

    private const string ReviewerName2 = "市场总监";
    private const string ReviewerInstructions2 = """
        你是一位市场总监，对广告文案有着丰富的经验，尤其擅长结合中国传统文化和现代营销理念。
        目标是确定给定的文案是否符合品牌调性并适合发布。
        如果符合，请声明说：已批准。
        如果不符合，仅提供如何改进文案的建议，不要提供具体示例。
        """;

    private const string CopyWriterName2 = "文案策划";
    private const string CopyWriterInstructions2 = """
        你是一位拥有十年经验的文案策划，擅长用简洁有力的语言打动消费者，尤其熟悉中国市场的文化背景。
        目标是作为该领域的专家，提炼并决定出最佳文案。
        每次回复只提供一个提案。
        你专注于手头的目标。
        不要浪费时间闲聊。
        在改进想法时考虑建议。
        """;

    public async Task UseAgentGroupChatWithTwoAgentsAsync()
    {
        // 定义代理
        ChatCompletionAgent agentReviewer = new()
        {
            Instructions = ReviewerInstructions,
            Name = ReviewerName,
            Kernel = ConfigExtensions.GetKernel("DouBao"),
        };

        ChatCompletionAgent agentWriter = new()
        {
            Instructions = CopyWriterInstructions,
            Name = CopyWriterName,
            Kernel = ConfigExtensions.GetKernel("DeepSeek"),
        };
        // 创建一个用于代理交互的聊天。
        AgentGroupChat chat = new(agentWriter, agentReviewer)
        {
            ExecutionSettings = new()
            {
                // 这里使用了一个 TerminationStrategy 子类，当助手的消息包含 "approve" 时将终止。
                TerminationStrategy = new ApprovalTerminationStrategy()
                {
                    // 只有艺术总监可以批准。
                    Agents = [agentReviewer],
                    // 限制总轮次
                    MaximumIterations = 10,
                },
            },
        };

        ChatMessageContent input = new(AuthorRole.User, "concept: maps made out of egg cartons.");
        chat.AddChatMessage(input);
        input.WriteAgentChatMessage();

        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
            response.WriteAgentChatMessage();
        }

        Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
    }

    public async Task UseAgentGroupChatWithTwoAgentsForCNAsync()
    {
        // 定义代理
        ChatCompletionAgent agentReviewer = new()
        {
            Instructions = ReviewerInstructions2,
            Name = ReviewerName2,
            Kernel = ConfigExtensions.GetKernel("DouBao"),
        };

        ChatCompletionAgent agentWriter = new()
        {
            Instructions = CopyWriterInstructions2,
            Name = CopyWriterName2,
            Kernel = ConfigExtensions.GetKernel("DouBao"),
        };
        AgentGroupChat chat = new(agentWriter, agentReviewer)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new ApprovalTerminationStrategyForCN()
                {
                    // 只有总监可以批准。
                    Agents = [agentReviewer],
                    // 限制总轮次
                    MaximumIterations = 10,
                },
            },
        };

        ChatMessageContent input = new(AuthorRole.User, "概念：用中国传统剪纸艺术设计的品牌广告。");
        chat.AddChatMessage(input);
        input.WriteAgentChatMessage();

        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
            response.WriteAgentChatMessage();
        }

        Console.WriteLine($"\n[是否完成: {chat.IsComplete}]");
    }

    private sealed class ApprovalTerminationStrategy : TerminationStrategy
    {
        // 当最后一条消息包含 "approve" 时终止
        protected override Task<bool> ShouldAgentTerminateAsync(
            Agent agent,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                history[^1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase)
                    ?? false
            );
    }

    private sealed class ApprovalTerminationStrategyForCN : TerminationStrategy
    {
        // 当最后一条消息包含 "通过"或"批准" 时终止
        protected override Task<bool> ShouldAgentTerminateAsync(
            Agent agent,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken
        )
        {
            var content = history[^1].Content;
            return Task.FromResult(
                content?.Contains("通过") == true || content?.Contains("批准") == true
            );
        }
    }
}


/*
# user: concept: maps made out of egg cartons.

# Assistant - CopyWriter: Egg carton maps: Unusual terrain for your imaginative voyages.

# Assistant - ArtDirector: This copy is rather simplistic and lacks the persuasive or engaging elements that would make it stand out. It could be refined by elaborating on the unique features or benefits of these egg carton maps. For example: "Egg
carton maps: Transform ordinary egg cartons into extraordinary guides that unlock new dimensions of imaginative exploration."

# Assistant - CopyWriter: Egg carton maps: Unleash your creativity as mundane cartons morph into captivating cartographic wonders.

# Assistant - ArtDirector: This copy has potential but could be even stronger. It's a bit wordy and the language could be more direct and vivid.

Perhaps something like: "Egg carton maps: Spark your imagination with cartons transformed into magical maps of discovery."

# Assistant - CopyWriter: Egg carton maps: Where ordinary cartons become extraordinary paths to adventure.

# Assistant - ArtDirector: This copy is approved. It presents a clear and inviting image of the egg carton maps in an engaging way.

[IS COMPLETED: True]
*/
