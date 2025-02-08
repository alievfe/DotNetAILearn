namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04;

/// <summary>
/// <see cref="Step04_AgentOrchestration"/> 示例中使用的流程事件
/// </summary>
public static class AgentOrchestrationEvents
{
    /// <summary>
    /// 开始流程事件标识
    /// </summary>
    public static readonly string StartProcess = nameof(StartProcess);

    /// <summary>
    /// 代理响应事件标识
    /// </summary>
    public static readonly string AgentResponse = nameof(AgentResponse);

    /// <summary>
    /// 代理已响应事件标识
    /// </summary>
    public static readonly string AgentResponded = nameof(AgentResponded);

    /// <summary>
    /// 代理正在处理事件标识
    /// </summary>
    public static readonly string AgentWorking = nameof(AgentWorking);

    /// <summary>
    /// 群组输入事件标识
    /// </summary>
    public static readonly string GroupInput = nameof(GroupInput);

    /// <summary>
    /// 群组消息事件标识
    /// </summary>
    public static readonly string GroupMessage = nameof(GroupMessage);

    /// <summary>
    /// 群组处理完成事件标识
    /// </summary>
    public static readonly string GroupCompleted = nameof(GroupCompleted);
}
