using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 模拟步骤，用于模拟用户信用评分检查，基于出生日期判断分数是否足够或不足
/// </summary>
public class CreditScoreCheckStep : KernelProcessStep
{
    public static class Functions
    {
        public const string DetermineCreditScore = nameof(DetermineCreditScore);
    }

    // 最低信用评分标准
    private const int MinCreditScore = 600;

    [KernelFunction(Functions.DetermineCreditScore)]
    public async Task DetermineCreditScoreAsync(
        KernelProcessStepContext context,
        NewCustomerForm customerDetails,
        Kernel _kernel
    )
    {
        // 根据用户的出生日期设定信用评分：如果为"02/03/1990"则设为700，否则设为500
        var creditScore = customerDetails.UserDateOfBirth == "02/03/1990" ? 700 : 500;

        if (creditScore >= MinCreditScore)
        {
            /*[信用检查] 信用评分检查通过*/
            Console.WriteLine("[CREDIT CHECK] Credit Score Check Passed");
            // 如果信用评分达到最低要求，则发送批准事件
            await context.EmitEventAsync(
                new() { Id = AccountOpeningEvents.CreditScoreCheckApproved, Data = true }
            );
            return;
        }
        /*[信用检查] 信用评分检查未通过*/
        Console.WriteLine("[CREDIT CHECK] Credit Score Check Failed");
        // 如果信用评分低于最低要求，则发送拒绝事件，并附上详细信息
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.CreditScoreCheckRejected,
                /* 很遗憾地通知您，您的信用评分为 {creditScore}，不足以申请PRIME ABC类型的账户" */
                Data =
                    $"We regret to inform you that your credit score of {creditScore} is insufficient to apply for an account of the type PRIME ABC",

                Visibility = KernelProcessEventVisibility.Public,
            }
        );
    }
}
