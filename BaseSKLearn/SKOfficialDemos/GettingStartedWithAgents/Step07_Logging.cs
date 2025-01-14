using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils;
using SKUtils.SKExtensions;
using SKUtils.TestUtils;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 重复 <see cref="Step03_Chat"/> 的示例，但通过将 <see cref="LoggerFactory"/> 赋值给 <see cref="AgentChat.LoggerFactory"/> 启用了日志记录。
/// </summary>
/// <remarks>
/// 由于日志记录始终启用，示例会变得非常冗长。
/// </remarks>
public class Step07_Logging(ITestOutputHelper output) : BaseAgentsTest(output)
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

    [Fact]
    public async Task UseLoggerFactoryWithAgentGroupChatAsync()
    {
        // 定义代理
        ChatCompletionAgent agentReviewer = new()
        {
            Instructions = ReviewerInstructions,
            Name = ReviewerName,
            Kernel = ConfigExtensions.GetKernel("DouBao"),
            LoggerFactory = this.LoggerFactory,
        };

        ChatCompletionAgent agentWriter = new()
        {
            Instructions = CopyWriterInstructions,
            Name = CopyWriterName,
            Kernel = ConfigExtensions.GetKernel("DeepSeek"),
            LoggerFactory = this.LoggerFactory,
        };
        // 创建一个用于代理交互的聊天。
        AgentGroupChat chat = new(agentWriter, agentReviewer)
        {
            LoggerFactory = this.LoggerFactory,
            ExecutionSettings = new()
            {
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
}
/*
[AddChatMessages] Adding Messages: 1.
[AddChatMessages] Added Messages: 1.

# user: concept: maps made out of egg cartons.
[InvokeAsync] Invoking chat: Microsoft.SemanticKernel.Agents.ChatCompletionAgent:a1875f45-5249-49ef-93b7-8d927ae4887a, Microsoft.SemanticKernel.Agents.ChatCompletionAgent:e789e896-e118-4cfc-94a2-abe8dc387631
[InvokeAsync] Selecting agent: Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy.
[NextAsync] Selected agent (0 / 2): a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAsync] Agent selected Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a by Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy
[InvokeAsync] Invoking chat: Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAgentAsync] Invoking agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a.
[InvokeAgentAsync] Creating channel for Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAgentAsync] Created channel for Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAsync] Agent #a1875f45-5249-49ef-93b7-8d927ae4887a Invoking service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService.
[InvokeAsync] Agent #a1875f45-5249-49ef-93b7-8d927ae4887a Invoked service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService with message count: 1.
[InvokeAgentAsync] Agent message Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a: "Navigate life’s twists and turns with egg-cellent precision—maps crafted from upcycled egg cartons. Because getting lost is for scrambled eggs.".

# Assistant - CopyWriter: "Navigate life’s twists and turns with egg-cellent precision—maps crafted from upcycled egg cartons. Because getting lost is for scrambled eggs."
[InvokeAgentAsync] Invoked agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a.
[ShouldTerminateAsync] Evaluating termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a.
[ShouldTerminateAsync] Microsoft.SemanticKernel.Agents.ChatCompletionAgent agent out of scope for termination: a1875f45-5249-49ef-93b7-8d927ae4887a.
[InvokeAsync] Yield chat - IsComplete: False
[InvokeAsync] Selecting agent: Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy.
[NextAsync] Selected agent (1 / 2): e789e896-e118-4cfc-94a2-abe8dc387631
[InvokeAsync] Agent selected Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631 by Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy
[InvokeAsync] Invoking chat: Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631
[InvokeAgentAsync] Invoking agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631.
[InvokeAsync] Agent #e789e896-e118-4cfc-94a2-abe8dc387631 Invoking service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService.
[InvokeAsync] Agent #e789e896-e118-4cfc-94a2-abe8dc387631 Invoked service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService with message count: 1.
[InvokeAgentAsync] Agent message Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631: This copy could be improved by adding more specific details about the unique features or benefits of these egg carton maps. For example, mention how the texture or shape of the cartons adds to the charm or functionality of the maps. Also, perhaps include a call to action, such as "Come discover the world anew with our egg carton masterpieces!" to engage the reader more effectively. .

# Assistant - ArtDirector: This copy could be improved by adding more specific details about the unique features or benefits of these egg carton maps. For example, mention how the texture or shape of the cartons adds to the charm or functionality of the maps. Also, perhaps include a call to action, such as "Come discover the world anew with our egg carton masterpieces!" to engage the reader more effectively. 
[InvokeAgentAsync] Invoked agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631.
[ShouldTerminateAsync] Evaluating termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631.
[ShouldTerminateAsync] Evaluated termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631 - False
[InvokeAsync] Yield chat - IsComplete: False
[InvokeAsync] Selecting agent: Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy.
[NextAsync] Selected agent (0 / 2): a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAsync] Agent selected Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a by Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy
[InvokeAsync] Invoking chat: Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAgentAsync] Invoking agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a.
[InvokeAsync] Agent #a1875f45-5249-49ef-93b7-8d927ae4887a Invoking service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService.
[InvokeAsync] Agent #a1875f45-5249-49ef-93b7-8d927ae4887a Invoked service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService with message count: 1.
[InvokeAgentAsync] Agent message Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a: "Explore the world with eco-chic flair—maps crafted from upcycled egg cartons. Each fold and crease tells a story, turning navigation into a tactile adventure. Because getting lost is for scrambled eggs.".

# Assistant - CopyWriter: "Explore the world with eco-chic flair—maps crafted from upcycled egg cartons. Each fold and crease tells a story, turning navigation into a tactile adventure. Because getting lost is for scrambled eggs."
[InvokeAgentAsync] Invoked agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a.
[ShouldTerminateAsync] Evaluating termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a.
[ShouldTerminateAsync] Microsoft.SemanticKernel.Agents.ChatCompletionAgent agent out of scope for termination: a1875f45-5249-49ef-93b7-8d927ae4887a.
[InvokeAsync] Yield chat - IsComplete: False
[InvokeAsync] Selecting agent: Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy.
[NextAsync] Selected agent (1 / 2): e789e896-e118-4cfc-94a2-abe8dc387631
[InvokeAsync] Agent selected Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631 by Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy
[InvokeAsync] Invoking chat: Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631
[InvokeAgentAsync] Invoking agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631.
[InvokeAsync] Agent #e789e896-e118-4cfc-94a2-abe8dc387631 Invoking service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService.
[InvokeAsync] Agent #e789e896-e118-4cfc-94a2-abe8dc387631 Invoked service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService with message count: 1.
[InvokeAgentAsync] Agent message Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631: This copy is acceptable to print. It evokes a sense of exploration and adventure while highlighting the eco-friendly aspect and unique texture of the egg carton maps. .

# Assistant - ArtDirector: This copy is acceptable to print. It evokes a sense of exploration and adventure while highlighting the eco-friendly aspect and unique texture of the egg carton maps. 
[InvokeAgentAsync] Invoked agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631.
[ShouldTerminateAsync] Evaluating termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631.
[ShouldTerminateAsync] Evaluated termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631 - False
[InvokeAsync] Yield chat - IsComplete: False
[InvokeAsync] Selecting agent: Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy.
[NextAsync] Selected agent (0 / 2): a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAsync] Agent selected Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a by Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy
[InvokeAsync] Invoking chat: Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a
[InvokeAgentAsync] Invoking agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a.
[InvokeAsync] Agent #a1875f45-5249-49ef-93b7-8d927ae4887a Invoking service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService.
[InvokeAsync] Agent #a1875f45-5249-49ef-93b7-8d927ae4887a Invoked service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService with message count: 1.
[InvokeAgentAsync] Agent message Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a: "Chart your course with eco-friendly charm—maps made from upcycled egg cartons. Every ridge and groove adds character to your journey. Because getting lost is for scrambled eggs.".

# Assistant - CopyWriter: "Chart your course with eco-friendly charm—maps made from upcycled egg cartons. Every ridge and groove adds character to your journey. Because getting lost is for scrambled eggs."
[InvokeAgentAsync] Invoked agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/a1875f45-5249-49ef-93b7-8d927ae4887a.
[ShouldTerminateAsync] Evaluating termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: a1875f45-5249-49ef-93b7-8d927ae4887a.
[ShouldTerminateAsync] Microsoft.SemanticKernel.Agents.ChatCompletionAgent agent out of scope for termination: a1875f45-5249-49ef-93b7-8d927ae4887a.
[InvokeAsync] Yield chat - IsComplete: False
[InvokeAsync] Selecting agent: Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy.
[NextAsync] Selected agent (1 / 2): e789e896-e118-4cfc-94a2-abe8dc387631
[InvokeAsync] Agent selected Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631 by Microsoft.SemanticKernel.Agents.Chat.SequentialSelectionStrategy
[InvokeAsync] Invoking chat: Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631
[InvokeAgentAsync] Invoking agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631.
[InvokeAsync] Agent #e789e896-e118-4cfc-94a2-abe8dc387631 Invoking service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService.
[InvokeAsync] Agent #e789e896-e118-4cfc-94a2-abe8dc387631 Invoked service Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatCompletionService with message count: 1.
[InvokeAgentAsync] Agent message Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631: This copy is approved. It effectively conveys the idea of using eco-friendly materials for mapping and creates an inviting tone with the description of the carton's features. .

# Assistant - ArtDirector: This copy is approved. It effectively conveys the idea of using eco-friendly materials for mapping and creates an inviting tone with the description of the carton's features. 
[InvokeAgentAsync] Invoked agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent/e789e896-e118-4cfc-94a2-abe8dc387631.
[ShouldTerminateAsync] Evaluating termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631.
[ShouldTerminateAsync] Evaluated termination for agent Microsoft.SemanticKernel.Agents.ChatCompletionAgent: e789e896-e118-4cfc-94a2-abe8dc387631 - True
[InvokeAsync] Yield chat - IsComplete: True
[InvokeAsync] Yield chat - IsComplete: True

[IS COMPLETED: True]

*/