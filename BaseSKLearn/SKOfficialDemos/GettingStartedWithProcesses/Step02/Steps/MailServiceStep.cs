using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 模拟步骤，使用用户消息模拟邮件服务。
/// </summary>
public class MailServiceStep : KernelProcessStep
{
    public static class Functions
    {
        public const string SendMailToUserWithDetails = nameof(SendMailToUserWithDetails);
    }

    [KernelFunction(Functions.SendMailToUserWithDetails)]
    public async Task SendMailServiceAsync(KernelProcessStepContext context, string message)
    {
        Console.WriteLine("======== MAIL SERVICE ======== ");
        Console.WriteLine(message);
        Console.WriteLine("============================== ");

        // 触发邮件发送事件，通知邮件已发送，并携带邮件内容
        await context.EmitEventAsync(
            new() { Id = AccountOpeningEvents.MailServiceSent, Data = message }
        );
    }
}
