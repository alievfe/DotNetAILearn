using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.SharedSteps;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;
using Microsoft.SemanticKernel;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02;

/// <summary>
/// 测试可见，InternLM DouBao pro完全不行。新的DouBao1.5可行, DeepSeek应该也可行。
/// 展示开户流程循环、扇入、扇出。
/// 演示如何创建 <see cref="KernelProcess"/> 并获取其对五条明确用户消息的响应。<br/>
/// 每个测试都有一组不同的用户消息，这些消息将触发相同的流程中的不同步骤。<br/>
/// 有关流程的可视化参考，请查看 <see href="https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/README.md#step02a_accountOpening" >流程图</see>。
/// </summary>
public class Step02a_AccountOpening
{
    /// <summary>
    /// 设置账户开户流程。
    /// </summary>
    /// <typeparam name="TUserInputStep">用户输入步骤的类型。</typeparam>
    /// <returns>返回配置好的 <see cref="KernelProcess"/>。</returns>
    private KernelProcess SetupAccountOpeningProcess<TUserInputStep>()
        where TUserInputStep : ScriptedUserInputStep
    {
        ProcessBuilder process = new("AccountOpeningProcess");
        var newCustomerFormStep = process.AddStepFromType<CompleteNewCustomerFormStep>();
        var userInputStep = process.AddStepFromType<TUserInputStep>();
        var displayAssistantMessageStep = process.AddStepFromType<DisplayAssistantMessageStep>();
        var customerCreditCheckStep = process.AddStepFromType<CreditScoreCheckStep>();
        var fraudDetectionCheckStep = process.AddStepFromType<FraudDetectionStep>();
        var mailServiceStep = process.AddStepFromType<MailServiceStep>();
        var coreSystemRecordCreationStep = process.AddStepFromType<NewAccountStep>();
        var marketingRecordCreationStep = process.AddStepFromType<NewMarketingEntryStep>();
        var crmRecordStep = process.AddStepFromType<CRMRecordCreationStep>();
        var welcomePacketStep = process.AddStepFromType<WelcomePacketStep>();

        process
            .OnInputEvent(AccountOpeningEvents.StartProcess)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    newCustomerFormStep,
                    CompleteNewCustomerFormStep.Functions.NewAccountWelcome
                )
            );

        // 当欢迎消息生成时，发送消息到 displayAssistantMessageStep（所有的消息都给这个步骤处理输出，返回触发公共事件AssistantResponseGenerated）
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.NewCustomerFormWelcomeMessageComplete)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    displayAssistantMessageStep,
                    DisplayAssistantMessageStep.Functions.DisplayAssistantMessage
                )
            );
        // 在显示任何助手消息后，用户输入将发送到 userInputStep
        displayAssistantMessageStep
            .OnEvent(CommonEvents.AssistantResponseGenerated)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    userInputStep,
                    ScriptedUserInputStep.Functions.GetUserInput
                )
            );
        // 当 userInput 步骤发出用户输入事件时，将其发送到 newCustomerForm 步骤
        // 当步骤有多个公共函数时，函数名称是必需的，例如 CompleteNewCustomerFormStep: NewAccountWelcome 和 NewAccountProcessUserInfo
        userInputStep
            .OnEvent(CommonEvents.UserInputReceived)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    newCustomerFormStep,
                    CompleteNewCustomerFormStep.Functions.NewAccountProcessUserInfo,
                    "userMessage"
                )
            );
        // 用户消息发送完成退出
        userInputStep.OnEvent(CommonEvents.Exit).StopProcess();

        // 当 newCustomerForm 步骤发出需要更多详细信息时，也把回复消息发送到 displayAssistantMessage 步骤打印
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.NewCustomerFormNeedsMoreDetails)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    displayAssistantMessageStep,
                    DisplayAssistantMessageStep.Functions.DisplayAssistantMessage
                )
            );

        // 当 newCustomerForm 完成时，1.依次传递执行如下，检测完成后实际创建账户
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.NewCustomerFormCompleted)
            // 信息传递到核心系统记录创建步骤
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    customerCreditCheckStep,
                    functionName: CreditScoreCheckStep.Functions.DetermineCreditScore,
                    parameterName: "customerDetails"
                )
            )
            // 信息传递到欺诈检测步骤进行验证
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    fraudDetectionCheckStep,
                    functionName: FraudDetectionStep.Functions.FraudDetectionCheck,
                    parameterName: "customerDetails"
                )
            )
            // 信息传递到核心系统记录创建步骤
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    coreSystemRecordCreationStep,
                    functionName: NewAccountStep.Functions.CreateNewAccount,
                    parameterName: "customerDetails"
                )
            );

        // 当 newCustomerForm 完成时，2.用户与用户的交互记录传递到核心系统记录创建步骤
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.CustomerInteractionTranscriptReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    coreSystemRecordCreationStep,
                    functionName: NewAccountStep.Functions.CreateNewAccount,
                    parameterName: "interactionTranscript"
                )
            );

        // 当 creditScoreCheck 步骤结果为拒绝时，信息传递到 mailService 步骤，通知用户申请状态及原因
        customerCreditCheckStep
            .OnEvent(AccountOpeningEvents.CreditScoreCheckRejected)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    mailServiceStep,
                    functionName: MailServiceStep.Functions.SendMailToUserWithDetails,
                    parameterName: "message"
                )
            );

        // 当 creditScoreCheck 步骤结果为批准时，信息传递到 fraudDetection 步骤以启动该步骤
        customerCreditCheckStep
            .OnEvent(AccountOpeningEvents.CreditScoreCheckApproved)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    fraudDetectionCheckStep,
                    functionName: FraudDetectionStep.Functions.FraudDetectionCheck,
                    parameterName: "previousCheckSucceeded"
                )
            );

        // 当 fraudDetectionCheck 步骤失败时，信息传递到 mailService 步骤，通知用户申请状态及原因
        fraudDetectionCheckStep
            .OnEvent(AccountOpeningEvents.FraudDetectionCheckFailed)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    mailServiceStep,
                    functionName: MailServiceStep.Functions.SendMailToUserWithDetails,
                    parameterName: "message"
                )
            );

        // 当 fraudDetectionCheck 步骤通过时，信息传递到核心系统记录创建步骤以启动该步骤
        fraudDetectionCheckStep
            .OnEvent(AccountOpeningEvents.FraudDetectionCheckPassed)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    coreSystemRecordCreationStep,
                    functionName: NewAccountStep.Functions.CreateNewAccount,
                    parameterName: "previousCheckSucceeded"
                )
            );

        // 当 coreSystemRecordCreation 步骤成功创建新 accountId 时，它将通过 marketingRecordCreation 步骤触发新营销条目的创建
        coreSystemRecordCreationStep
            .OnEvent(AccountOpeningEvents.NewMarketingRecordInfoReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    marketingRecordCreationStep,
                    functionName: NewMarketingEntryStep.Functions.CreateNewMarketingEntry,
                    parameterName: "userDetails"
                )
            );

        // 当 coreSystemRecordCreation 步骤成功创建新 accountId 时，它将通过 crmRecord 步骤触发新 CRM 条目的创建
        coreSystemRecordCreationStep
            .OnEvent(AccountOpeningEvents.CRMRecordInfoReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    crmRecordStep,
                    functionName: CRMRecordCreationStep.Functions.CreateCRMEntry,
                    parameterName: "userInteractionDetails"
                )
            );

        // 当步骤有多个输入参数时，parameterName 是必需的，例如 welcomePacketStep.CreateWelcomePacketAsync
        // 当 coreSystemRecordCreation 步骤成功创建新 accountId 时，它将账户信息传递到 welcomePacket 步骤
        coreSystemRecordCreationStep
            .OnEvent(AccountOpeningEvents.NewAccountDetailsReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(welcomePacketStep, parameterName: "accountDetails")
            );

        // 当 marketingRecordCreation 步骤成功创建新营销条目时，它将通知 welcomePacket 步骤准备就绪
        marketingRecordCreationStep
            .OnEvent(AccountOpeningEvents.NewMarketingEntryCreated)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    welcomePacketStep,
                    parameterName: "marketingEntryCreated"
                )
            );

        // 当 crmRecord 步骤成功创建新 CRM 条目时，它将通知 welcomePacket 步骤准备就绪
        crmRecordStep
            .OnEvent(AccountOpeningEvents.CRMRecordInfoEntryCreated)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    welcomePacketStep,
                    parameterName: "crmRecordCreated"
                )
            );

        // 在 crmRecord 和 marketing 创建后，将创建欢迎包，然后通过 mailService 步骤向用户发送信息
        welcomePacketStep
            .OnEvent(AccountOpeningEvents.WelcomePacketCreated)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    mailServiceStep,
                    functionName: MailServiceStep.Functions.SendMailToUserWithDetails,
                    parameterName: "message"
                )
            );

        // 所有可能的路径最终都会通过 mailServiceStep 完成通知用户账户创建决定。
        // 最后发出MailServiceSent结束
        mailServiceStep.OnEvent(AccountOpeningEvents.MailServiceSent).StopProcess();

        KernelProcess kernelProcess = process.Build();

        return kernelProcess;
    }

    /// <summary>
    /// 此测试使用特定的userId和出生日期(DOB)，使信用评分和欺诈检测通过
    /// </summary>
    public async Task UseAccountOpeningProcessSuccessfulInteractionAsync()
    {
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        KernelProcess kernelProcess =
            SetupAccountOpeningProcess<UserInputSuccessfulInteractionStep>();
        using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = AccountOpeningEvents.StartProcess, Data = null }
        );
    }

    /*
    ASSISTANT: Hello there, I can help you out fill out the information needed to open a new account with us.
    Please provide some personal information like first name and last name to get started.

    USER: I would like to open an account
    ASSISTANT: Great! To open the account, we need some details from you. Could you please tell me your first name, last name, date of birth in the format MM/DD/YYYY, the two-letter abbreviation of the state you live in (for example, NY for New York), your phone number in the format (XXX)-XXX-XXXX, your user ID in the format XXX-XXX-XXXX, and your email address? Thanks!

    USER: My name is John Contoso, dob 02/03/1990
    ASSISTANT: That's great, John Contoso. But we still need to know the state you live in (in 2-letter Uppercase State Abbreviation format), your phone number (in the format (XXX)-XXX-XXXX), your user ID (in the format XXX-XXX-XXXX), and your email address.      

    USER: I live in Washington and my phone number es 222-222-1234
    PS D:\Works\Test\dot-net-ai-learn>  & 'c:\Users\Administrator\.vscode-insiders\extensions\ms-dotnettools.csharp-2.61.28-win32-x64\.debugger\x86_64\vsdbg.exe' '--interpreter=vscode' '--connection=5415876961dc4546acafd05b54e9fd3a' 
    Hello, World!
    ASSISTANT: Hello there, I can help you out fill out the information needed to open a new account with us.
    Please provide some personal information like first name and last name to get started.

    USER: I would like to open an account
    ASSISTANT: Great! To start the account - opening process, I'll need some information from you. First, could you please tell me your first name and last name? For example, if your name is John Doe, "John" would be your first name and "Doe" would be your last name.

    USER: My name is John Contoso, dob 02/03/1990
    ASSISTANT: Thank you for providing your name and date of birth. Now, I need to know the 2 - letter uppercase abbreviation of the USA state you live in. For example, if you live in California, the state abbreviation is "CA". Also, could you share your phone number (in the format like (123)-456 - 7890) and your user ID (in the format 123 - 456 - 7890)? Additionally, please provide your email address.

    USER: I live in Washington and my phone number es 222-222-1234
    ASSISTANT: Thank you for providing your state of residence and phone number. I've recorded that you live in Washington (WA) and your phone number is (222)-222-1234.

    Now, I still need your user ID (in the format 123 - 456 - 7890) and your email address. For example, a valid user ID could be 345 
    - 678 - 9012, and an email address might look like johndoe@example.com.

    USER: My userId is 987-654-3210
    ASSISTANT: Great! I've noted down your user ID as 987 - 654 - 3210. The only remaining field is your email address. Please provide an email address in a valid format, like example@domain.com.

    USER: My email is john.contoso@contoso.com, what else do you need?
    [NEW_USER_FORM_COMPLETED]: {"userFirstName":"John","userLastName":"Contoso","userDateOfBirth":"02/03/1990","userState":"WA","userPhoneNumber":"(222)-222-1234","userId":"987-654-3210","userEmail":"john.contoso@contoso.com"}
    [CREDIT CHECK] Credit Score Check Passed
    [FRAUD CHECK] Fraud Check Passed
    [ACCOUNT CREATION] New Account 00000000-0000-0000-0000-000000000000 created
    [MARKETING ENTRY CREATION] New Account 00000000-0000-0000-0000-000000000000 created
    [CRM ENTRY CREATION] New Account 00000000-0000-0000-0000-000000000000 created
    [WELCOME PACKET] New Account 00000000-0000-0000-0000-000000000000 created
    ======== MAIL SERVICE ======== 
    Dear John Contoso
    We are thrilled to inform you that you have successfully created a new PRIME ABC Account with us!

    Account Details:
    Account Number: 00000000-0000-0000-0000-000000000000
    Account Type: PrimeABC

    Please keep this confidential for security purposes.

    Here is the contact information we have in file:

    Email: john.contoso@contoso.com
    Phone: (222)-222-1234

    Thank you for opening an account with us!
    ==============================
    */

    /// <summary>
    /// 此测试使用特定的出生日期(DOB)，使信用评分失败
    /// </summary>
    public async Task UseAccountOpeningProcessFailureDueToCreditScoreFailureAsync()
    {
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        KernelProcess kernelProcess =
            SetupAccountOpeningProcess<UserInputCreditScoreFailureInteractionStep>();
        using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = AccountOpeningEvents.StartProcess, Data = null }
        );
    }

    /// <summary>
    /// 此测试使用特定的userId，使欺诈检测失败
    /// </summary>
    public async Task UseAccountOpeningProcessFailureDueToFraudFailureAsync()
    {
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        KernelProcess kernelProcess =
            SetupAccountOpeningProcess<UserInputFraudFailureInteractionStep>();
        using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = AccountOpeningEvents.StartProcess, Data = null }
        );
    }
}
