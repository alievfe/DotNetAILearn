using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;

/// <summary>
/// 模拟步骤，模拟新账户创建后触发其他服务的过程。
/// </summary>
public class NewAccountStep : KernelProcessStep
{
    public static class Functions
    {
        public const string CreateNewAccount = nameof(CreateNewAccount);
    }

    [KernelFunction(Functions.CreateNewAccount)]
    public async Task CreateNewAccountAsync(
        KernelProcessStepContext context,
        bool previousCheckSucceeded,
        NewCustomerForm customerDetails,
        List<ChatMessageContent> interactionTranscript,
        Kernel _kernel
    )
    {
        // 调用API创建用户的新账户（此处为占位符）。
        var accountId = new Guid();
        AccountDetails accountDetails = new()
        {
            UserDateOfBirth = customerDetails.UserDateOfBirth,
            UserFirstName = customerDetails.UserFirstName,
            UserLastName = customerDetails.UserLastName,
            UserId = customerDetails.UserId,
            UserPhoneNumber = customerDetails.UserPhoneNumber,
            UserState = customerDetails.UserState,
            UserEmail = customerDetails.UserEmail,
            AccountId = accountId,
            AccountType = AccountType.PrimeABC, // 设置账户类型为PrimeABC。
        };

        Console.WriteLine($"[ACCOUNT CREATION] New Account {accountId} created");
        // Log the creation of the new account.

        // 发出营销记录准备事件。
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.NewMarketingRecordInfoReady,
                Data = new MarketingNewEntryDetails
                {
                    AccountId = accountId,
                    Name = $"{customerDetails.UserFirstName} {customerDetails.UserLastName}",
                    PhoneNumber = customerDetails.UserPhoneNumber,
                    Email = customerDetails.UserEmail,
                },
            }
        );

        // 发出CRM记录准备事件。
        await context.EmitEventAsync(
            new()
            {
                Id = AccountOpeningEvents.CRMRecordInfoReady,
                Data = new AccountUserInteractionDetails
                {
                    AccountId = accountId,
                    UserInteractionType = UserInteractionType.OpeningNewAccount,
                    InteractionTranscript = interactionTranscript,
                },
            }
        );

        // 完成，发出新账户详情准备事件。
        await context.EmitEventAsync(
            new() { Id = AccountOpeningEvents.NewAccountDetailsReady, Data = accountDetails }
        );
    }
}
