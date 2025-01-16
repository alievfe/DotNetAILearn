using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 步骤用于处理样本过程：
/// - Step_02_AccountOpening.cs
/// 显示助手消息
/// </summary>
public class DisplayAssistantMessageStep : KernelProcessStep
{
    public static class Functions
    {
        public const string DisplayAssistantMessage = nameof(DisplayAssistantMessage);
    }

    [KernelFunction(Functions.DisplayAssistantMessage)]
    public async ValueTask DisplayAssistantMessageAsync(
        KernelProcessStepContext context,
        string assistantMessage
    )
    {
        // 设置控制台文本颜色为蓝色，以便于区分助手的消息。
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"ASSISTANT: {assistantMessage}\n");

        // 重置控制台文本颜色为其默认值。
        Console.ResetColor();

        // 发出一个事件，表明已经生成了助手的响应。
        await context.EmitEventAsync(
            new() { Id = CommonEvents.AssistantResponseGenerated, Data = assistantMessage }
        );
    }
}
