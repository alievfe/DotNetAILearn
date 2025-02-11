using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils;
using SKUtils.SKExtensions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示如何使用 <see cref="KernelFunctionTerminationStrategy"/> 和 <see cref="KernelFunctionSelectionStrategy"/>
/// 来管理 <see cref="AgentGroupChat"/> 的执行。
/// 自定义策略管理多个代理的交互，适用于需要协作和动态控制的 AI 任务场景。
/// </summary>
public class Step04_KernelFunctionStrategies
{
    private const string ModelName = "DeepSeek";
    private const string ReviewerName = "ArtDirector";

    /*
        你是一位艺术总监，对文案写作有着独到的见解，深受 David Ogilvy 的影响。
        目标是确定给定的文案是否可以印刷。
        如果可以，请声明已批准。
        如果不可以，请提供如何改进文案的建议，但不要提供示例。
    */
    private const string ReviewerInstructions = """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine if the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without examples.
        """;

    private const string CopyWriterName = "CopyWriter";

    /*
        你是一位拥有十年经验的文案撰写人，以简洁和冷幽默著称。
        目标是作为该领域的专家，提炼并决定出最佳文案。
        每次回复只提供一个提案。
        不要用引号分隔回复。
        你专注于手头的目标。
        不要浪费时间闲聊。
        在改进想法时考虑建议。
    */
    private const string CopyWriterInstructions = """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        Never delimit the response with quotation marks.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """;

    public async Task UseKernelFunctionStrategiesWithAgentGroupChatAsync()
    {
        // 定义代理
        ChatCompletionAgent agentReviewer = new()
        {
            Instructions = ReviewerInstructions,
            Name = ReviewerName,
            Kernel = ConfigExtensions.GetKernel(ModelName),
        };
        ChatCompletionAgent agentWriter = new()
        {
            Instructions = CopyWriterInstructions,
            Name = CopyWriterName,
            Kernel = ConfigExtensions.GetKernel(ModelName),
        };

        // 确定文案是否已获批准。如果是，请用单个单词回复：yes
        KernelFunction terminationFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            """
            Determine if the copy has been approved.  If so, respond with a single word: yes

            History:
            {{$history}}
            """,
            safeParameterNames: "history"
        );
        /*
         根据最近的参与者确定对话中的下一个参与者。
        只声明下一个参与者的名字。
        任何参与者都不应连续发言两次。

        只能从以下参与者中选择：
        - {{{ReviewerName}}}
        - {{{CopyWriterName}}}
        
        选择下一个参与者时始终遵循以下规则：
        - 在 {{{CopyWriterName}}} 之后，轮到 {{{ReviewerName}}}。
        - 在 {{{ReviewerName}}} 之后，轮到 {{{CopyWriterName}}}。
        */
        KernelFunction selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
            Determine which participant takes the next turn in a conversation based on the the most recent participant.
            State only the name of the participant to take the next turn.
            No participant should take more than one turn in a row.

            Choose only from these participants:
            - {{{ReviewerName}}}
            - {{{CopyWriterName}}}

            Always follow these rules when selecting the next participant:
            - After {{{CopyWriterName}}}, it is {{{ReviewerName}}}'s turn.
            - After {{{ReviewerName}}}, it is {{{CopyWriterName}}}'s turn.

            History:
            {{$history}}
            """,
            safeParameterNames: "history"
        );

        // 将用于选择和终止的历史记录限制为最近的消息。
        ChatHistoryTruncationReducer strategyReducer = new(1);

        // 创建一个用于代理交互的聊天。
        AgentGroupChat chat = new(agentWriter, agentReviewer)
        {
            ExecutionSettings = new()
            {
                // 这里 KernelFunctionTerminationStrategy 会在艺术总监批准时终止。
                TerminationStrategy = new KernelFunctionTerminationStrategy(
                    terminationFunction,
                    ConfigExtensions.GetKernel(ModelName)
                )
                {
                    // 只有艺术总监可以批准。
                    Agents = [agentReviewer],
                    // 自定义结果解析器，用于确定响应是否为 "yes"
                    ResultParser = (result) =>
                        result
                            .GetValue<string>()
                            ?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                    // 提示变量名称，用于历史记录参数。
                    HistoryVariableName = "history",
                    // 限制总轮次
                    MaximumIterations = 10,
                    // 通过不包含整个历史记录来节省 token
                    HistoryReducer = strategyReducer,
                },
                // 这里 KernelFunctionSelectionStrategy 根据提示函数选择代理。
                SelectionStrategy = new KernelFunctionSelectionStrategy(
                    selectionFunction,
                    ConfigExtensions.GetKernel(ModelName)
                )
                {
                    // 始终从撰写人代理开始。
                    InitialAgent = agentWriter,
                    // 返回整个结果值作为字符串。
                    ResultParser = (result) => result.GetValue<string>() ?? CopyWriterName,
                    // 提示变量名称，用于历史记录参数。
                    HistoryVariableName = "history",
                    // 通过不包含整个历史记录来节省 token
                    HistoryReducer = strategyReducer,
                    // 仅包含代理名称，不包含消息内容
                    EvaluateNameOnly = true,
                },
            },
        };

        // 调用聊天并显示消息。
        // 概念：用鸡蛋纸盒制作的地图。
        ChatMessageContent message = new(AuthorRole.User, "concept: maps made out of egg cartons.");
        chat.AddChatMessage(message);
        message.WriteAgentChatMessage();

        await foreach (ChatMessageContent responese in chat.InvokeAsync())
        {
            responese.WriteAgentChatMessage();
        }

        Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
    }
}

/*
调用过程：首先用户输入，然后初始由agentWriter开始，输出后调用SelectionStrategy的kf生成下一个要输出的代理的名称，ArtDirector。ArtDirector输出通过，每次ArtDirector输出后调用TerminationStrategy的kf判断是否通过，通过则生成yes被结果解析，实现终止。
# user: concept: maps made out of egg cartons.

# Assistant - CopyWriter: Navigate breakfast with egg-carton cartography—where every bump is a hill and every divot a valley.

# Assistant - ArtDirector: Approved. The copy is clever, engaging, and aligns well with the concept. It effectively communicates the idea in a playfu
a playful and memorable way.

[IS COMPLETED: True]
*/