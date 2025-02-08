using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.SharedSteps;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Plugins;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Steps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04;

/// <summary>
/// 经过测试，现版本function calling设置为Auto，但是不加入任何Plugin时，国产大模型api全部报错InvalidParameter，使用GPT-4o中文运行失败，英文可行；同时也不支持json_schema格式化响应结果
/// 1.启动时触发用户输入模拟 2.用户输入后激活主代理处理3.主代理根据需求调用日历/天气代理组4.代理组内部通过策略函数控制对话流转5.最终结果返回主代理并渲染输出
/// 重点：1.每次Manager响应用户后对history进行意图判断，涉及对大模型的json_schema响应格式化，反序列化为Indent对象决定下一步发出事件名称。2.每次启用agentGroup，先对之前用户和Manager的chatHistory总结，作为其输入。
/// 演示了将_Agent Framework_与进程集成，这包括直接的_agent_交互以及使用_AgentGroupChat_
/// 演示创建一个 <see cref="KernelProcess"/>，用于编排 <see cref="Agent"/> 对话。
/// 有关该流程的可视化参考，请查看 <see href="https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/README.md#step04_agentorchestration">图表</see>。
/// </summary>
public class Step04_AgentOrchestration
{
    /// <summary>
    /// 编排一个单独的代理收集用户输入，然后委托给一组代理。
    /// 这组代理向该单独的代理提供响应，该单独的代理继续与用户交互。
    /// </summary>
    [Fact]
    public async Task DelegatedGroupChatAsync()
    {
        // 定义流程
        KernelProcess process = SetupAgentProcess<BasicAgentChatUserInput>(
            nameof(DelegatedGroupChatAsync)
        );

        // 执行流程
        await RunProcessAsync(process);
    }
    /*
    [00:00] USER: Hi
    [00:06] ASSISTANT - Manager: Hello! How can I help you with your scheduling request today?
    [00:08] USER: List the upcoming events on my calendar for the next week
    [00:25] ASSISTANT - Manager: To proceed, can you confirm you'd like me to work on listing your upcoming events for the next week?
    [00:27] USER: Correct
    [00:40] ASSISTANT - Manager: Got it! I’m working on your request now.
    [00:57]         USER: List my upcoming calendar events for the next week.
    [00:57]         ASSISTANT - CalendarAgent: Here is your upcoming calendar event for the next week:

    - **Title:** Haircut Appointment
      **Date:** 2025-02-16
      **Description:** Haircut and style at the salon.
    [00:57] ASSISTANT - Manager: You have a haircut appointment scheduled for February 16, 2025. It includes a haircut and style at the salon.
    [00:59] USER: When is an open time to go camping near home for 4 days after the end of this week?
    [01:31] ASSISTANT - Manager: Can you confirm that I should check for a 4-day open window for camping after the end of this week?
    [01:47] USER: Yes, and I'd prefer nice weather.
    [02:17] ASSISTANT - Manager: Thank you for confirming! I’m working on finding a 4-day open window for camping after this week, considering nice weather.
    [03:16]         USER: Find a 4-day open window for camping after this week with nice weather.
    [03:16]         ASSISTANT - CalendarAgent: Here are the available open windows to consider for a 4-day camping trip, factoring in your current scheduled events after this week:

    1. **February 18–20**: This window is open and does not conflict with any events.
    2. **February 22–25**: Another suitable period without overlapping scheduled events.
    3. **March 2–5**: Potential 4-day stretch without planned activities.

    Would you like me to confirm the weather for any of these options?
    [03:16]         ASSISTANT - WeatherAgent: Here are the weather conditions for each of the 4-day camping windows:

    1. **February 18–21**:
       - **High Temperature:** 88°F, **Low Temperature:** 69°F
       - **Precipitation Chance:** 77% (may not be ideal due to rain chances)

    2. **February 22–25**:
       - **High Temperature:** 76°F, **Low Temperature:** 60°F
       - **Precipitation Chance:** 57% (slightly better conditions)

    3. **March 2–5**:
       - **High Temperature:** 57°F, **Low Temperature:** 44°F
       - **Precipitation Chance:** 56% (colder, but rain chances are moderate).

    The second window (February 22–25) seems the most favorable for camping, with slightly cooler temperatures and a relatively moderate chance of rain. Would you like me to assist with preparations or provide more details?
    [03:16] ASSISTANT - Manager: For a 4-day camping trip with nice weather after this week, the best period appears to be February 22–25. During this time, temperatures will be around 76°F during the day and 60°F at night, with a 57% chance of rain, which is relatively moderate compared to other options. Let me know if you need help with preparations or more information!
    [03:18] USER: Sounds good, add the soonest option without conflicts to my calendar
    [03:21] ASSISTANT - Manager: Just to confirm, would you like me to schedule the camping trip for February 22–25, considering it’s the earliest open period with nice weather?
    [03:23] USER: Correct
    [03:29] ASSISTANT - Manager: Got it! I'm working on scheduling your camping trip for February 22–25 now.
    [03:29] DONE!

    [00:00] 用户：嗨
    [00:06] 客服代表：您好！今天我能在日程安排方面为您提供什么帮助呢？
    [00:08] 用户：列出我日历上下周即将到来的活动
    [00:25] 客服代表：为了继续操作，您能确认一下是要我列出您下周即将到来的活动吗？
    [00:27] 用户：没错
    [00:40] 客服代表：明白了！我现在就处理您的请求。
    [00:57]     用户：列出我日历上下周即将到来的活动。
    [00:57]     日历助手客服代表：这是您下周即将到来的日历活动：
    - **活动名称**：理发预约
      **日期**：2025 - 02 - 16
      **活动描述**：在美发沙龙理发和做造型。
    [00:57] 客服代表：您在2025年2月16日有一个理发预约。包括在美发沙龙理发和做造型。
    [00:59] 用户：本周结束后，什么时候有连续4天的空闲时间可以在家附近露营？
    [01:31] 客服代表：您能确认一下我要查找本周结束后连续4天的露营空闲时间吗？
    [01:47] 用户：是的，我希望天气晴朗。
    [02:17] 客服代表：感谢您的确认！我正在查找本周之后适合露营的连续4天空闲时间，同时会考虑天气晴朗的因素。
    [03:16]     用户：查找本周之后天气晴朗且有连续4天空闲的露营时间段。
    [03:16]     日历助手客服代表：考虑到您本周之后已有的日程安排，以下是可供考虑的连续4天露营空闲时间段：
    1. **2月18 - 20日**：这个时间段空闲，且与任何日程都不冲突。
    2. **2月22 - 25日**：另一个合适的时间段，没有与已安排的日程重叠。
    3. **3月2 - 5日**：潜在的连续4天空闲时间段，没有计划中的活动。
    您需要我确认其中某个时间段的天气情况吗？
    [03:16]     天气助手客服代表：以下是每个连续4天露营时间段的天气情况：
    1. **2月18 - 21日**：
       - **最高气温**：88华氏度，**最低气温**：69华氏度
       - **降雨概率**：77%（由于降雨概率较高，可能不太理想）
    2. **2月22 - 25日**：
       - **最高气温**：76华氏度，**最低气温**：60华氏度
       - **降雨概率**：57%（天气条件稍好）
    3. **3月2 - 5日**：
       - **最高气温**：57华氏度，**最低气温**：44华氏度
       - **降雨概率**：56%（较冷，但降雨概率适中）
    第二个时间段（2月22 - 25日）似乎最适合露营，气温稍凉且降雨概率相对适中。您需要我协助做准备工作或提供更多详情吗？
    [03:16] 客服代表：本周之后，适合4天露营且天气较好的最佳时间段似乎是2月22 - 25日。在此期间，白天的气温约为76华氏度，夜间约为60华氏度，降雨概率为57%，与其他时间段相比，这个概率相对适中。如果您需要准备方面的帮助或更多信息，请告诉我！
    [03:18] 用户：听起来不错，把最早且没有冲突的选项添加到我的日历中。
    [03:21] 客服代表：为了确认一下，考虑到2月22 - 25日是最早且天气晴朗的空闲时间段，您希望我把这次露营行程安排进日程吗？
    [03:23] 用户：没错
    [03:29] 客服代表：明白了！我现在就为您把2月22 - 25日的露营行程安排进日程。
    [03:29] 完成！ 
    */
    private sealed class BasicAgentChatUserInput : ScriptedUserInputStep
    {
        public BasicAgentChatUserInput()
        {
            this.SuppressOutput = true;
        }

        public override void PopulateUserInputs(UserInputState state)
        {
            state.UserInputs.Add("你好");
            state.UserInputs.Add("列出我下周日历上即将发生的事件");
            state.UserInputs.Add("正确");
            state.UserInputs.Add("本周结束后，什么时候有连续 4 天的空闲时间可以在家附近露营？");
            state.UserInputs.Add("是的，我更喜欢好天气。");
            state.UserInputs.Add("听起来不错，将最早且无冲突的选项添加到我的日历中");
            state.UserInputs.Add("正确");
            state.UserInputs.Add("就这些，谢谢");
        }
    }

    private async Task RunProcessAsync(KernelProcess process)
    {
        // 初始化服务
        ChatHistory history = [];
        Kernel kernel = SetupKernel(history);

        // 执行流程
        using LocalKernelProcessContext localProcess = await process.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = AgentOrchestrationEvents.StartProcess }
        );

        // 演示历史记录独立于流程状态进行维护
        Console.WriteLine(new string('-', 80));

        foreach (ChatMessageContent message in history)
        {
            RenderMessageStep.Render(message);
        }
    }

    private KernelProcess SetupAgentProcess<TUserInputStep>(string processName)
        where TUserInputStep : ScriptedUserInputStep
    {
        ProcessBuilder process = new(processName);

        var userInputStep = process.AddStepFromType<TUserInputStep>();
        var renderMessageStep = process.AddStepFromType<RenderMessageStep>();
        var managerAgentStep = process.AddStepFromType<ManagerAgentStep>();
        var agentGroupStep = process.AddStepFromType<AgentGroupChatStep>();

        // 为这些步骤设置错误处理
        AttachErrorStep(userInputStep, ScriptedUserInputStep.Functions.GetUserInput);
        AttachErrorStep(
            managerAgentStep,
            ManagerAgentStep.Functions.InvokeAgent,
            ManagerAgentStep.Functions.InvokeGroup,
            ManagerAgentStep.Functions.ReceiveResponse
        );
        AttachErrorStep(agentGroupStep, AgentGroupChatStep.Functions.InvokeAgentGroup);

        // 入口点
        process
            .OnInputEvent(AgentOrchestrationEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

        // 将用户输入传递给主代理
        userInputStep
            .OnEvent(CommonEvents.UserInputReceived)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    managerAgentStep,
                    ManagerAgentStep.Functions.InvokeAgent
                )
            )
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    renderMessageStep,
                    RenderMessageStep.Functions.RenderUserText,
                    parameterName: "message"
                )
            );

        // 流程完成
        userInputStep
            .OnEvent(CommonEvents.UserInputComplete)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    renderMessageStep,
                    RenderMessageStep.Functions.RenderDone
                )
            )
            .StopProcess();

        // 渲染主代理的响应
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.AgentResponse)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    renderMessageStep,
                    RenderMessageStep.Functions.RenderMessage,
                    parameterName: "message"
                )
            );

        // 请求完成
        managerAgentStep
            .OnEvent(CommonEvents.UserInputComplete)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    renderMessageStep,
                    RenderMessageStep.Functions.RenderDone
                )
            )
            .StopProcess();

        // 请求更多用户输入
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.AgentResponded)
            .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

        // 委托给内部代理
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.AgentWorking)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    managerAgentStep,
                    ManagerAgentStep.Functions.InvokeGroup
                )
            );

        // 向内部代理提供输入
        managerAgentStep
            .OnEvent(AgentOrchestrationEvents.GroupInput)
            .SendEventTo(new ProcessFunctionTargetBuilder(agentGroupStep, parameterName: "input"));

        // 渲染内部聊天的响应（为了可见性）
        agentGroupStep
            .OnEvent(AgentOrchestrationEvents.GroupMessage)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    renderMessageStep,
                    RenderMessageStep.Functions.RenderInnerMessage,
                    parameterName: "message"
                )
            );

        // 向主代理提供内部响应
        agentGroupStep
            .OnEvent(AgentOrchestrationEvents.GroupCompleted)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    managerAgentStep,
                    ManagerAgentStep.Functions.ReceiveResponse,
                    parameterName: "response"
                )
            );

        KernelProcess kernelProcess = process.Build();

        return kernelProcess;

        // 当有异常时，渲染错误消息并停止流程
        void AttachErrorStep(ProcessStepBuilder step, params string[] functionNames)
        {
            foreach (string functionName in functionNames)
            {
                step.OnFunctionError(functionName)
                    .SendEventTo(
                        new ProcessFunctionTargetBuilder(
                            renderMessageStep,
                            RenderMessageStep.Functions.RenderError,
                            "error"
                        )
                    )
                    .StopProcess();
            }
        }
    }

    private Kernel SetupKernel(ChatHistory history)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        // 向内核添加聊天完成功能
        var configRoot = ConfigExtensions.LoadConfigFromJson();
        var chatConfig = configRoot.GetSection("Token-GPT").Get<OpenAIConfig>();
        builder.AddOpenAIChat(chatConfig);

        // 将代理注入服务集合
        SetupAgents(builder, builder.Build());
        // 将历史记录提供程序注入服务集合
        builder.Services.AddSingleton<IChatHistoryProvider>(new ChatHistoryProvider(history));

        // 注意：取消注释以查看流程日志
        builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();

        return builder.Build();
    }

    private const string ManagerInstructions = """
        捕获用户为其日程安排请求提供的信息。
        请求确认，无需提供额外细节建议。
        确认后，告知用户你正在处理请求。
        永远不要直接回答用户的请求。
        """;

    private const string CalendarInstructions = """
        根据当前指示评估已安排的日历事件。
        在没有指定日期的情况下，优先考虑最早的机会，但不将评估限制在当前日期。
        永远不要考虑或提议与现有事件冲突的日程安排。
        """;

    private const string WeatherInstructions = """
        根据当前指示提供天气信息。
        """;

    private const string ManagerSummaryInstructions = """
        以第一人称命令形式总结最近的用户请求。
        """;

    private const string SuggestionSummaryInstructions = """
        直接向用户总结响应内容。
        """;

    private static void SetupAgents(IKernelBuilder builder, Kernel kernel)
    {
        // 创建主代理并将其注入服务集合
        ChatCompletionAgent managerAgent = CreateAgent(
            "Manager",
            ManagerInstructions,
            kernel.Clone()
        );
        builder.Services.AddKeyedSingleton(ManagerAgentStep.AgentServiceKey, managerAgent);

        // 创建群组聊天并将其注入服务集合
        SetupGroupChat(builder, kernel);

        // 创建缩减器并将其注入服务集合
        builder.Services.AddKeyedSingleton(
            ManagerAgentStep.ReducerServiceKey,
            SetupReducer(kernel, ManagerSummaryInstructions)
        );
        builder.Services.AddKeyedSingleton(
            AgentGroupChatStep.ReducerServiceKey,
            SetupReducer(kernel, SuggestionSummaryInstructions)
        );
    }

    private static ChatHistorySummarizationReducer SetupReducer(
        Kernel kernel,
        string instructions
    ) =>
        new(kernel.GetRequiredService<IChatCompletionService>(), 1)
        {
            SummarizationInstructions = instructions,
        };

    private static void SetupGroupChat(IKernelBuilder builder, Kernel kernel)
    {
        const string CalendarAgentName = "CalendarAgent";
        ChatCompletionAgent calendarAgent = CreateAgent(
            CalendarAgentName,
            CalendarInstructions,
            kernel.Clone()
        );
        calendarAgent.Kernel.Plugins.AddFromType<CalendarPlugin>();

        const string WeatherAgentName = "WeatherAgent";
        ChatCompletionAgent weatherAgent = CreateAgent(
            WeatherAgentName,
            WeatherInstructions,
            kernel.Clone()
        );
        weatherAgent.Kernel.Plugins.AddFromType<WeatherPlugin>();
        weatherAgent.Kernel.Plugins.AddFromType<LocationPlugin>();

        KernelFunction selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
            根据最近的参与者确定对话中下一个发言的参与者。
            只说出下一个发言的参与者的名称。
            任何参与者都不应连续发言。

            只能从以下参与者中选择：
            - {{{CalendarAgentName}}}
            - {{{WeatherAgentName}}}

            选择下一个参与者时，始终遵循以下规则：
            - 用户输入后，轮到 {{{CalendarAgentName}}} 发言。
            - {{{CalendarAgentName}}} 发言后，轮到 {{{WeatherAgentName}}} 发言。
            - {{{WeatherAgentName}}} 发言后，轮到 {{{CalendarAgentName}}} 发言。

            历史记录：
            {{$history}}
            """,
            safeParameterNames: "history"
        );

        KernelFunction terminationFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
            评估用户最近的日历请求是否已收到最终响应。
            如果请求了天气条件，{{{WeatherAgentName}}} 必须提供输入。

            如果满足所有这些条件，请回复一个单词：yes

            历史记录：
            {{$history}}
            """,
            safeParameterNames: "history"
        );

        AgentGroupChat chat = new(calendarAgent, weatherAgent)
        {
            // 注意：在示例之外使用时，请替换日志记录器。
            // 使用 `this.LoggerFactory` 作为示例的一部分观察日志输出。
            LoggerFactory = NullLoggerFactory.Instance,
            ExecutionSettings = new()
            {
                SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                {
                    HistoryVariableName = "history",
                    HistoryReducer = new ChatHistoryTruncationReducer(1),
                    ResultParser = (result) => result.GetValue<string>() ?? calendarAgent.Name!,
                },
                TerminationStrategy = new KernelFunctionTerminationStrategy(
                    terminationFunction,
                    kernel
                )
                {
                    HistoryVariableName = "history",
                    MaximumIterations = 12,
                    //HistoryReducer = new ChatHistoryTruncationReducer(2),
                    ResultParser = (result) =>
                        result
                            .GetValue<string>()
                            ?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                },
            },
        };
        builder.Services.AddSingleton(chat);
    }

    private static ChatCompletionAgent CreateAgentWithoutFC(
        string name,
        string instructions,
        Kernel kernel
    ) =>
        new()
        {
            Name = name,
            Instructions = instructions,
            Kernel = kernel.Clone(),
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings { Temperature = 0 }),
        };

    private static ChatCompletionAgent CreateAgent(
        string name,
        string instructions,
        Kernel kernel
    ) =>
        new()
        {
            Name = name,
            Instructions = instructions,
            Kernel = kernel.Clone(),
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    Temperature = 0,
                }
            ),
        };
}
