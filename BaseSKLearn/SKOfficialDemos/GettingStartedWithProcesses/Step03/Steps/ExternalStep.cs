using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;

/// <summary>
/// 流程示例中使用的步骤：
/// - Step_03_FoodPreparation.cs
/// </summary>
public class ExternalStep(string externalEventName) : KernelProcessStep
{
    private readonly string _externalEventName = externalEventName;

    [KernelFunction]
    public async Task EmitExternalEventAsync(KernelProcessStepContext context, object data)
    {
        await context.EmitEventAsync(
            new()
            {
                Id = this._externalEventName,
                Data = data,
                Visibility = KernelProcessEventVisibility.Public,
            }
        );
    }
}
