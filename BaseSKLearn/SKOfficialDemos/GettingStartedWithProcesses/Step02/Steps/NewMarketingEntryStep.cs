using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 模拟步骤，模拟为营销目的创建新用户条目的过程。
/// </summary>
public class NewMarketingEntryStep : KernelProcessStep
{
    public static class Functions
    {
        public const string CreateNewMarketingEntry = nameof(CreateNewMarketingEntry);
    }

    [KernelFunction(Functions.CreateNewMarketingEntry)]
    public async Task CreateNewMarketingEntryAsync(
        KernelProcessStepContext context,
        MarketingNewEntryDetails userDetails,
        Kernel _kernel
    )
    {
        // 记录营销条目创建日志信息。
        Console.WriteLine(
            $"[MARKETING ENTRY CREATION] New Account {userDetails.AccountId} created"
        );

        // 调用API为营销目的创建用户的条目（此处为占位符）。
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.NewMarketingEntryCreated,
                Data = true, // 这里使用布尔值true作为示例数据，实际应用中可能需要更详细的信息。
            }
        );
    }
}
