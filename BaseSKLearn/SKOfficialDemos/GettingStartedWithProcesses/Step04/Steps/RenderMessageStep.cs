using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Steps;

/// <summary>
/// 向用户显示输出。在当前示例中，只是将内容写入控制台，
/// 但在实际场景中，这将是一个更复杂的渲染系统。将此渲染逻辑与其他流程步骤的内部逻辑分离，
/// 可以简化职责约定，并且便于进行测试和状态管理。
/// </summary>
public class RenderMessageStep : KernelProcessStep
{
    public static class Functions
    {
        public const string RenderDone = nameof(RenderMessageStep.RenderDone);
        public const string RenderError = nameof(RenderMessageStep.RenderError);
        public const string RenderInnerMessage = nameof(RenderMessageStep.RenderInnerMessage);
        public const string RenderMessage = nameof(RenderMessageStep.RenderMessage);
        public const string RenderUserText = nameof(RenderMessageStep.RenderUserText);
    }

    private static readonly Stopwatch s_timer = Stopwatch.StartNew();

    /// <summary>
    /// 渲染一条明确的消息，以表明流程已按预期状态完成。
    /// </summary>
    /// <remarks>
    /// 如果未渲染此消息，则认为流程失败。
    /// </remarks>
    [KernelFunction]
    public void RenderDone()
    {
        Render("完成！");
    }

    /// <summary>
    /// 渲染异常信息
    /// </summary>
    [KernelFunction]
    public void RenderError(KernelProcessError error, ILogger logger)
    {
        string message = string.IsNullOrWhiteSpace(error.Message) ? "意外失败" : error.Message;
        Render($"错误: {message} [{error.GetType().Name}]{Environment.NewLine}{error.StackTrace}");
        logger.LogError("意外失败: {ErrorMessage} [{ErrorType}]", error.Message, error.Type);
    }

    /// <summary>
    /// 渲染用户输入
    /// </summary>
    [KernelFunction]
    public void RenderUserText(string message)
    {
        Render($"{AuthorRole.User.Label.ToUpperInvariant()}: {message}");
    }

    /// <summary>
    /// 渲染主聊天中的助手消息
    /// </summary>
    [KernelFunction]
    public void RenderMessage(ChatMessageContent message)
    {
        Render(message);
    }

    /// <summary>
    /// 渲染内部聊天中的助手消息
    /// </summary>
    [KernelFunction]
    public void RenderInnerMessage(ChatMessageContent message)
    {
        Render(message, indent: true);
    }

    public static void Render(ChatMessageContent message, bool indent = false)
    {
        string displayName = !string.IsNullOrWhiteSpace(message.AuthorName)
            ? $" - {message.AuthorName}"
            : string.Empty;
        Render(
            $"{(indent ? "\t" : string.Empty)}{message.Role.Label.ToUpperInvariant()}{displayName}: {message.Content}"
        );
    }

    public static void Render(string message)
    {
        Console.WriteLine($"[{s_timer.Elapsed:mm\\:ss}] {message}");
    }
}
