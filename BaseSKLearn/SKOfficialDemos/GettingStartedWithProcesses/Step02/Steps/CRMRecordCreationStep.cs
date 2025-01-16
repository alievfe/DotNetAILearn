using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 模拟步骤，模拟创建新的CRM条目。
/// </summary>
public class CRMRecordCreationStep : KernelProcessStep
{
    public static class Functions
    {
        public const string CreateCRMEntry = nameof(CreateCRMEntry);
    }

    [KernelFunction(Functions.CreateCRMEntry)]
    public async Task CreateCRMEntryAsync(KernelProcessStepContext context, AccountUserInteractionDetails userInteractionDetails, Kernel _kernel)
    {
        // 记录CRM条目创建日志信息。
        Console.WriteLine($"[CRM ENTRY CREATION] New Account {userInteractionDetails.AccountId} created");

        // 调用API创建新的CRM条目（此处为占位符）。
        await context.EmitEventAsync(new()
        {
            Id = AccountOpeningEvents.CRMRecordInfoEntryCreated,
            Data = true  // 这里使用布尔值true作为示例数据，实际应用中可能需要更详细的信息。
        });
    }
}