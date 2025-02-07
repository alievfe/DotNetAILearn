using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Events;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.SharedSteps;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Processes;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;
using Microsoft.SemanticKernel;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02;

/// <summary>
/// 如何重构流程，并将较小的流程用作较大流程中的步骤。
/// 演示如何创建 <see cref="KernelProcess"/> 并获取其对五个明确用户消息的响应。<br/>
/// 每个测试都有一组不同的用户消息，这些消息将使用相同的管道触发不同的步骤。<br/>
/// 有关流程的视觉参考，请查看 <see href="https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/README.md#step02a_accountOpening" >图表</see>。
/// </summary>
public class Step02b_AccountOpening
{
    private KernelProcess SetupAccountOpeningProcess<TUserInputStep>()
        where TUserInputStep : ScriptedUserInputStep
    {
        ProcessBuilder process = new("AccountOpeningProcessWithSubprocesses");

        // 添加新客户表单步骤
        var newCustomerFormStep = process.AddStepFromType<CompleteNewCustomerFormStep>();
        // 添加用户输入步骤
        var userInputStep = process.AddStepFromType<TUserInputStep>();
        // 添加显示助理消息步骤
        var displayAssistantMessageStep = process.AddStepFromType<DisplayAssistantMessageStep>();

        // 添加账户验证步骤，使用已有的process创建step，注意类型是ProcessBuilder而不是ProcessStepBuilder
        var accountVerificationStep = process.AddStepFromProcess(
            NewAccountVerificationProcess.CreateProcess()
        );
        // 添加账户创建步骤
        var accountCreationStep = process.AddStepFromProcess(
            NewAccountCreationProcess.CreateProcess()
        );

        // 添加邮件服务步骤
        var mailServiceStep = process.AddStepFromType<MailServiceStep>();

        // 当启动流程事件发生时...
        process
            .OnInputEvent(AccountOpeningEvents.StartProcess)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    newCustomerFormStep,
                    CompleteNewCustomerFormStep.Functions.NewAccountWelcome
                )
            );

        // 当欢迎消息生成后，发送消息到显示助理消息步骤
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.NewCustomerFormWelcomeMessageComplete)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    displayAssistantMessageStep,
                    DisplayAssistantMessageStep.Functions.DisplayAssistantMessage
                )
            );

        // 当用户输入步骤发出用户输入事件时，将信息传递给新客户表单步骤
        userInputStep
            .OnEvent(CommonEvents.UserInputReceived)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    newCustomerFormStep,
                    CompleteNewCustomerFormStep.Functions.NewAccountProcessUserInfo,
                    "userMessage"
                )
            );

        userInputStep.OnEvent(CommonEvents.Exit).StopProcess();

        // 当新客户表单步骤需要更多信息时，发送消息到显示助理消息步骤
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.NewCustomerFormNeedsMoreDetails)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    displayAssistantMessageStep,
                    DisplayAssistantMessageStep.Functions.DisplayAssistantMessage
                )
            );

        // 在任何助理消息显示后，预期用户输入下一步骤为用户输入步骤
        displayAssistantMessageStep
            .OnEvent(CommonEvents.AssistantResponseGenerated)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    userInputStep,
                    ScriptedUserInputStep.Functions.GetUserInput
                )
            );

        // 当新客户表单完成时...（这里使用ProcessBuilder类型的accountVerificationStep，需要使用WhereInputEventIs方法检索到对应监听事件）
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.NewCustomerFormCompleted)
            // 将信息传递给账户验证步骤
            .SendEventTo(
                accountVerificationStep.WhereInputEventIs(
                    AccountOpeningEvents.NewCustomerFormCompleted
                )
            )
            // 将信息传递给验证过程步骤
            .SendEventTo(
                accountCreationStep.WhereInputEventIs(AccountOpeningEvents.NewCustomerFormCompleted)
            );

        // 当新客户表单完成时，将用户交互转录与用户传递给核心系统记录创建步骤
        newCustomerFormStep
            .OnEvent(AccountOpeningEvents.CustomerInteractionTranscriptReady)
            .SendEventTo(
                accountCreationStep.WhereInputEventIs(
                    AccountOpeningEvents.CustomerInteractionTranscriptReady
                )
            );

        // 当信用评分检查结果为拒绝时，将信息传递给邮件服务步骤以通知用户申请状态及原因
        accountVerificationStep
            .OnEvent(AccountOpeningEvents.CreditScoreCheckRejected)
            .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep));

        // 当欺诈检测失败时，将信息传递给邮件服务步骤以通知用户申请状态及原因
        accountVerificationStep
            .OnEvent(AccountOpeningEvents.FraudDetectionCheckFailed)
            .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep));

        // 当欺诈检测通过时，将信息传递给核心系统记录创建步骤以启动此步骤
        accountVerificationStep
            .OnEvent(AccountOpeningEvents.FraudDetectionCheckPassed)
            .SendEventTo(
                accountCreationStep.WhereInputEventIs(
                    AccountOpeningEvents.NewAccountVerificationCheckPassed
                )
            );

        // 在CRM记录和营销记录创建完成后，创建一个欢迎包并通过邮件服务步骤向用户发送信息
        accountCreationStep
            .OnEvent(AccountOpeningEvents.WelcomePacketCreated)
            .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep));

        // 所有可能的路径最终都会通过邮件服务步骤完成，通知用户账户创建决定
        mailServiceStep.OnEvent(AccountOpeningEvents.MailServiceSent).StopProcess();

        KernelProcess kernelProcess = process.Build();

        return kernelProcess;
    }

    /// <summary>
    /// 本测试使用特定的userId和出生日期（DOB），使信用评分和欺诈检测均通过
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

    /// <summary>
    /// 本测试使用特定的出生日期（DOB），使信用评分失败
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
    /// 本测试使用特定的userId，使欺诈检测失败
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
