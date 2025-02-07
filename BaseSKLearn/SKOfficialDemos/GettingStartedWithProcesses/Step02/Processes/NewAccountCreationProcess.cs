namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Processes;

using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;
// 版权声明：版权所有 (c) Microsoft Corporation。保留所有权利。

using Microsoft.SemanticKernel;

/// <summary>
/// 演示如何创建<see cref="KernelProcess"/>以及
/// 引出其对五条明确用户消息的响应。<br/>
/// 对于每个测试，有一组不同的用户消息会触发使用相同流程的不同步骤。<br/>
/// 关于该过程的视觉参考，请查看<see href="https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/README.md#step02b_accountOpening">图表</see>。
/// </summary>
public static class NewAccountCreationProcess
{
    public static ProcessBuilder CreateProcess()
    {
        // 创建一个名为“AccountCreationProcess”的流程构建器
        ProcessBuilder process = new("AccountCreationProcess");

        // 添加核心系统记录创建步骤
        var coreSystemRecordCreationStep = process.AddStepFromType<NewAccountStep>();
        // 添加营销记录创建步骤
        var marketingRecordCreationStep = process.AddStepFromType<NewMarketingEntryStep>();
        // 添加CRM记录创建步骤
        var crmRecordStep = process.AddStepFromType<CRMRecordCreationStep>();
        // 添加欢迎包创建步骤
        var welcomePacketStep = process.AddStepFromType<WelcomePacketStep>();

        // 当新客户表单完成时...
        process
            .OnInputEvent(AccountOpeningEvents.NewCustomerFormCompleted)
            // 将信息传递给核心系统记录创建步骤
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    coreSystemRecordCreationStep,
                    functionName: NewAccountStep.Functions.CreateNewAccount,
                    parameterName: "customerDetails"
                )
            );

        // 当新客户表单完成时，将用户交互转录与用户传递给核心系统记录创建步骤
        process
            .OnInputEvent(AccountOpeningEvents.CustomerInteractionTranscriptReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    coreSystemRecordCreationStep,
                    functionName: NewAccountStep.Functions.CreateNewAccount,
                    parameterName: "interactionTranscript"
                )
            );

        // 当欺诈检测检查通过时，信息将被发送到核心系统记录创建步骤以启动此步骤
        process
            .OnInputEvent(AccountOpeningEvents.NewAccountVerificationCheckPassed)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    coreSystemRecordCreationStep,
                    functionName: NewAccountStep.Functions.CreateNewAccount,
                    parameterName: "previousCheckSucceeded"
                )
            );

        // 当核心系统记录创建步骤成功创建一个新的账户ID时，它将通过营销记录创建步骤触发新营销条目的创建
        coreSystemRecordCreationStep
            .OnEvent(AccountOpeningEvents.NewMarketingRecordInfoReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    marketingRecordCreationStep,
                    functionName: NewMarketingEntryStep.Functions.CreateNewMarketingEntry,
                    parameterName: "userDetails"
                )
            );

        // 当核心系统记录创建步骤成功创建一个新的账户ID时，它将通过CRM记录步骤触发新CRM条目的创建
        coreSystemRecordCreationStep
            .OnEvent(AccountOpeningEvents.CRMRecordInfoReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    crmRecordStep,
                    functionName: CRMRecordCreationStep.Functions.CreateCRMEntry,
                    parameterName: "userInteractionDetails"
                )
            );

        // 当核心系统记录创建步骤成功创建一个新的账户ID时，它将把账户详细信息传递给欢迎包步骤
        coreSystemRecordCreationStep
            .OnEvent(AccountOpeningEvents.NewAccountDetailsReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(welcomePacketStep, parameterName: "accountDetails")
            );

        // 当营销记录创建步骤成功创建一个新的营销条目时，它将通知欢迎包步骤已准备好
        marketingRecordCreationStep
            .OnEvent(AccountOpeningEvents.NewMarketingEntryCreated)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    welcomePacketStep,
                    parameterName: "marketingEntryCreated"
                )
            );

        // 当CRM记录步骤成功创建一个新的CRM条目时，它将通知欢迎包步骤已准备好
        crmRecordStep
            .OnEvent(AccountOpeningEvents.CRMRecordInfoEntryCreated)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    welcomePacketStep,
                    parameterName: "crmRecordCreated"
                )
            );

        return process;
    }
}
