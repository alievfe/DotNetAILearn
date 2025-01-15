using System;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;

/// <summary>
/// 处理共享步骤发出的事件。<br/>
/// /// </summary>
public static class CommonEvents
{
    public static readonly string UserInputReceived = nameof(UserInputReceived);
    public static readonly string UserInputComplete = nameof(UserInputComplete);
    public static readonly string AssistantResponseGenerated = nameof(AssistantResponseGenerated);
    public static readonly string Exit = nameof(Exit);
}
