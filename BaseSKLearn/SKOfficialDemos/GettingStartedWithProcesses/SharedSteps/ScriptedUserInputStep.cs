using System;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.SharedSteps;

/// <summary>
/// 一个用于获取用户输入的步骤。
///
/// 该步骤用于以下流程示例：
/// - Step01_Processes.cs
/// - Step02_AccountOpening.cs
/// - Step04_AgentOrchestration
/// </summary>
public class ScriptedUserInputStep : KernelProcessStep<UserInputState>
{
    public static class Functions
    {
        public const string GetUserInput = nameof(GetUserInput);
    }

    protected bool SuppressOutput { get; init; }

    /// <summary>
    /// 用户输入步骤的状态对象。该对象保存用户输入列表和当前输入索引。
    /// </summary>
    private UserInputState? _state;

    /// <summary>
    /// 用户可重写的方法，用于填充自定义的用户消息。
    /// </summary>
    /// <param name="state">已初始化的步骤状态对象。</param>
    public virtual void PopulateUserInputs(UserInputState state)
    {
        return;
    }

    /// <summary>
    /// 激活用户输入步骤，初始化状态对象。该方法在流程启动时调用，并在任何 KernelFunction 调用之前执行。
    /// </summary>
    /// <param name="state">步骤的状态对象。</param>
    /// <returns>返回一个 <see cref="ValueTask"/>。</returns>
    public override ValueTask ActivateAsync(KernelProcessStepState<UserInputState> state)
    {
        _state = state.State;

        // 填充用户输入
        PopulateUserInputs(_state!);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 获取下一个用户消息。
    /// </summary>
    /// <returns>返回用户输入的消息。</returns>
    internal string GetNextUserMessage()
    {
        if (
            _state != null
            && _state.CurrentInputIndex >= 0
            && _state.CurrentInputIndex < this._state.UserInputs.Count
        )
        {
            var userMessage = this._state!.UserInputs[_state.CurrentInputIndex];
            _state.CurrentInputIndex++;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"USER: {userMessage}");
            Console.ResetColor();

            return userMessage;
        }
        
        // 如果没有更多用户输入，则返回空字符串
        Console.WriteLine(
            "SCRIPTED_USER_INPUT: No more scripted user messages defined, returning empty string as user message"
        );
        return string.Empty;
    }

    /// <summary>
    /// 获取用户输入。
    /// 可以重写此方法以自定义发出的事件。
    /// </summary>
    /// <param name="context">一个 <see cref="KernelProcessStepContext"/> 实例，可用于从 KernelFunction 中发出事件。</param>
    /// <returns>返回一个 <see cref="ValueTask"/>。</returns>
    [KernelFunction(Functions.GetUserInput)]
    public virtual async ValueTask GetUserInputAsync(KernelProcessStepContext context)
    {
        var userMessage = this.GetNextUserMessage();

        // 如果用户输入为空，则发出退出事件
        if (string.IsNullOrEmpty(userMessage))
        {
            await context.EmitEventAsync(new() { Id = CommonEvents.Exit });
            return;
        }

        // 发出用户输入接收事件
        await context.EmitEventAsync(
            new() { Id = CommonEvents.UserInputReceived, Data = userMessage }
        );
    }
}

/// <summary>
/// <see cref="ScriptedUserInputStep"/> 的状态对象。
/// </summary>
public record class UserInputState
{
    /// <summary>
    /// 用户输入列表。
    /// </summary>
    public List<string> UserInputs { get; init; } = [];

    /// <summary>
    /// 当前用户输入的索引。
    /// </summary>
    public int CurrentInputIndex { get; set; } = 0;
}
