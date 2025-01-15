using System;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.SharedSteps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Process.Tools;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step01;

/// <summary>
/// 演示如何创建 <see cref="KernelProcess"/> 并获取其对三条明确用户消息的响应。
/// </summary>
public class Step01_Processes
{
    /// <summary>
    /// 演示如何创建一个包含多个步骤的简单流程，通过事件，该流程接收用户输入，与聊天完成服务交互，并展示流程中的循环。
    /// </summary>
    /// <returns>返回一个 <see cref="Task"/>。</returns>
    ///
    public async Task UseSimpleProcessAsync()
    {
        // 创建一个包含聊天完成服务的 Kernel
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");

        // 创建一个将与聊天完成服务交互的流程
        ProcessBuilder process = new("ChatBot");
        var introStep = process.AddStepFromType<IntroStep>(); // 添加介绍步骤
        var userInputStep = process.AddStepFromType<ChatUserInputStep>(); // 添加用户输入步骤
        var responseStep = process.AddStepFromType<ChatBotResponseStep>(); // 添加聊天机器人响应步骤
        
        // 定义流程在接收到外部事件时的行为
        process
            .OnInputEvent(ChatBotEvents.StartProcess) // 当接收到启动流程事件时
            .SendEventTo(new ProcessFunctionTargetBuilder(introStep)); // 将事件发送到介绍步骤
        // 当介绍步骤完成时，通知用户输入步骤
        introStep.OnFunctionResult().SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));
        // 当用户输入步骤发出退出事件时，将其发送到结束步骤
        userInputStep.OnEvent(ChatBotEvents.Exit).StopProcess();
        // 当用户输入步骤发出用户输入事件时，将其发送到助手响应步骤
        userInputStep
            .OnEvent(CommonEvents.UserInputReceived)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage")
            );
        // 当助手响应步骤发出响应事件时，将其发送到用户输入步骤
        responseStep
            .OnEvent(ChatBotEvents.AssistantResponseGenerated)
            .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

        // 构建流程以获取可以启动的句柄
        KernelProcess kernelProcess = process.Build();
        
        // 生成流程的 Mermaid 图并将其打印到控制台
        string mermaidGraph = kernelProcess.ToMermaid();
        Console.WriteLine($"=== Start - Mermaid Diagram for '{process.Name}' ===");
        Console.WriteLine(mermaidGraph);
        Console.WriteLine($"=== End - Mermaid Diagram for '{process.Name}' ===");

        // 从 Mermaid 图生成图像
        // string generatedImagePath = await MermaidRenderer.GenerateMermaidImageAsync(
        //     mermaidGraph,
        //     "ChatBotProcess.png"
        // );
        // Console.WriteLine($"Diagram generated at: {generatedImagePath}");

        // 使用初始外部事件启动流程
        using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = ChatBotEvents.StartProcess, Data = null }
        );
    }

    /// <summary>
    /// 最简单的流程步骤实现。IntroStep 用于打印介绍信息。
    /// </summary>
    private sealed class IntroStep : KernelProcessStep
    {
        /// <summary>
        /// 打印介绍信息到控制台。
        /// </summary>
        [KernelFunction]
        public void PrintIntroMessage()
        {
            Console.WriteLine("Welcome to Processes in Semantic Kernel.\n");
        }
    }

    /// <summary>
    /// 一个用于获取用户输入的步骤。
    /// </summary>
    private sealed class ChatUserInputStep : ScriptedUserInputStep
    {
        public override void PopulateUserInputs(UserInputState state)
        {
            // 预定义用户输入
            state.UserInputs.Add("Hello");
            state.UserInputs.Add("How tall is the tallest mountain?");
            state.UserInputs.Add("How low is the lowest valley?");
            state.UserInputs.Add("How wide is the widest river?");
            state.UserInputs.Add("exit");
            state.UserInputs.Add(
                "This text will be ignored because exit process condition was already met at this point."
            );
        }

        public override async ValueTask GetUserInputAsync(KernelProcessStepContext context)
        {
            var userMessage = this.GetNextUserMessage();
            if (string.Equals(userMessage, "exit", StringComparison.OrdinalIgnoreCase))
            {
                // 退出条件满足，发出退出事件
                await context.EmitEventAsync(new() { Id = ChatBotEvents.Exit, Data = userMessage });
                return;
            }

            // 发出用户输入接收事件
            await context.EmitEventAsync(
                new() { Id = CommonEvents.UserInputReceived, Data = userMessage }
            );
        }
    }

    /// <summary>
    /// 一个步骤，用于从聊天完成服务生成对用户输入的响应。
    /// </summary>
    private sealed class ChatBotResponseStep : KernelProcessStep<ChatBotState>
    {
        public static class Functions
        {
            public const string GetChatResponse = nameof(GetChatResponse);
        }

        /// <summary>
        /// 聊天机器人响应步骤的内部状态对象。
        /// </summary>
        internal ChatBotState? _state;

        /// <summary>
        /// ActivateAsync 是初始化步骤状态对象的地方。
        /// </summary>
        /// <param name="state">一个 <see cref="ChatBotState"/> 实例。</param>
        /// <returns>返回一个 <see cref="ValueTask"/>。</returns>
        public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// 从聊天完成服务生成响应。
        /// </summary>
        /// <param name="context">当前步骤和流程的上下文。<see cref="KernelProcessStepContext"/></param>
        /// <param name="userMessage">来自前一步骤的用户消息。</param>
        /// <param name="_kernel">一个 <see cref="Kernel"/> 实例。</param>
        /// <returns></returns>
        [KernelFunction(Functions.GetChatResponse)]
        public async Task GetChatResponseAsync(
            KernelProcessStepContext context,
            string userMessage,
            Kernel _kernel
        )
        {
            _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));
            IChatCompletionService chatService =
                _kernel.Services.GetRequiredService<IChatCompletionService>();
            ChatMessageContent response =
                await chatService
                    .GetChatMessageContentAsync(_state.ChatMessages)
                    .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    "Failed to get a response from the chat completion service."
                );
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"ASSISTANT: {response.Content}");
            Console.ResetColor();

            // 更新状态以包含响应
            _state.ChatMessages.Add(response);

            // 发出助手响应生成事件
            await context.EmitEventAsync(
                new KernelProcessEvent
                {
                    Id = ChatBotEvents.AssistantResponseGenerated,
                    Data = response,
                }
            );
        }
    }

    /// <summary>
    /// <see cref="ChatBotResponseStep"/> 的状态对象。
    /// </summary>
    private sealed class ChatBotState
    {
        internal ChatHistory ChatMessages { get; } = [];
    }

    /// <summary>
    /// 定义聊天机器人流程可以发出的事件的类。这不是必需的，但用于确保事件名称的一致性。
    /// </summary>
    private static class ChatBotEvents
    {
        public const string StartProcess = nameof(StartProcess);
        public const string IntroComplete = nameof(IntroComplete);
        public const string AssistantResponseGenerated = nameof(AssistantResponseGenerated);
        public const string Exit = nameof(Exit);
    }
}
