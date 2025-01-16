using System;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 模拟步骤，模拟在账户创建后为新用户生成欢迎包。
/// </summary>
public class WelcomePacketStep : KernelProcessStep
{
    public static class Functions
    {
        /// <summary>
        /// 函数名称常量，用于标识创建欢迎包的功能。
        /// </summary>
        public const string CreateWelcomePacket = nameof(CreateWelcomePacket);
    }

    [KernelFunction(Functions.CreateWelcomePacket)]
    public async Task CreateWelcomePacketAsync(
        KernelProcessStepContext context,
        bool marketingEntryCreated,
        bool crmRecordCreated,
        AccountDetails accountDetails,
        Kernel _kernel
    )
    {
        // 记录欢迎包创建日志信息。
        Console.WriteLine($"[WELCOME PACKET] New Account {accountDetails.AccountId} created");

        // 生成欢迎邮件内容。
        /*
            尊敬的 {accountDetails.UserFirstName} {accountDetails.UserLastName}：

            我们很高兴地通知您，您已成功在我们这里创建了一个新的PRIME ABC账户！

            账户详情：
            账户编号：{accountDetails.AccountId}
            账户类型：{accountDetails.AccountType}

            请妥善保管这些信息以确保安全。

            这是我们存档中的联系信息：

            邮箱：{accountDetails.UserEmail}
            电话：{accountDetails.UserPhoneNumber}

            感谢您选择在我们这里开户！
            */
        var mailMessage = $"""
            Dear {accountDetails.UserFirstName} {accountDetails.UserLastName}
            We are thrilled to inform you that you have successfully created a new PRIME ABC Account with us!

            Account Details:
            Account Number: {accountDetails.AccountId}
            Account Type: {accountDetails.AccountType}

            Please keep this confidential for security purposes.

            Here is the contact information we have in file:

            Email: {accountDetails.UserEmail}
            Phone: {accountDetails.UserPhoneNumber}

            Thank you for opening an account with us!
            """;

        // 发出欢迎包创建事件。
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.WelcomePacketCreated,
                Data = mailMessage,
                Visibility = KernelProcessEventVisibility.Public, // 设置事件的可见性为公共。
            }
        );
    }
}
