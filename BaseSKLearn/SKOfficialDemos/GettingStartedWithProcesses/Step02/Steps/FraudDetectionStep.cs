using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 模拟步骤，用于模拟基于用户ID的欺诈检测检查，根据用户ID的不同，欺诈检测会通过或不通过。
/// </summary>
public class FraudDetectionStep : KernelProcessStep
{
    public static class Functions
    {
        // 函数名：FraudDetectionCheck，表示欺诈检测检查
        public const string FraudDetectionCheck = nameof(FraudDetectionCheck);
    }

    [KernelFunction(Functions.FraudDetectionCheck)]
    public async Task FraudDetectionCheckAsync(
        KernelProcessStepContext context,
        bool previousCheckSucceeded,
        NewCustomerForm customerDetails,
        Kernel _kernel
    )
    {
        // 如果用户ID为"123-456-7890"，则欺诈检查不通过
        if (customerDetails.UserId == "123-456-7890")
        {
            Console.WriteLine("[FRAUD CHECK] Fraud Check Failed");
            await context.EmitEventAsync(
                new()
                {
                    Id = AccountOpeningEvents.FraudDetectionCheckFailed,
                    // 很遗憾地通知您，我们发现您提供的关于申请PRIME ABC类型新账户的信息中存在一些不一致之处。
                    Data =
                        "We regret to inform you that we found some inconsistent details regarding the information you provided regarding the new account of the type PRIME ABC you applied.",
                    Visibility = KernelProcessEventVisibility.Public,
                }
            );
            return;
        }
        // 欺诈检测通过
        Console.WriteLine("[FRAUD CHECK] Fraud Check Passed");
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.FraudDetectionCheckPassed,
                Data = true,
                Visibility = KernelProcessEventVisibility.Public,
            }
        );
    }
}
