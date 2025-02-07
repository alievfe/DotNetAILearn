using System;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Steps;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step02.Processes;

/// <summary>
/// 演示如何创建<see cref="KernelProcess"/>以及
/// 引出其对五条明确用户消息的响应。<br/>
/// 对于每个测试，有一组不同的用户消息会触发使用相同流程的不同步骤。<br/>
/// 关于该过程的视觉参考，请查看<see href="https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/README.md#step02b_accountOpening">图表</see>。
/// </summary>
public static class NewAccountVerificationProcess
{
    public static ProcessBuilder CreateProcess()
    {
        // 创建一个名为“AccountVerificationProcess”的流程构建器
        ProcessBuilder process = new("AccountVerificationProcess");

        // 添加信用评分检查步骤
        var customerCreditCheckStep = process.AddStepFromType<CreditScoreCheckStep>();
        // 添加欺诈检测步骤
        var fraudDetectionCheckStep = process.AddStepFromType<FraudDetectionStep>();

        // 当新客户表单完成时...（这里让这个process监听NewCustomerFormCompleted事件）
        process
            .OnInputEvent(AccountOpeningEvents.NewCustomerFormCompleted) // 监听新客户表单完成事件
            // 将信息传递给核心系统记录创建步骤
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    customerCreditCheckStep,
                    functionName: CreditScoreCheckStep.Functions.DetermineCreditScore,
                    parameterName: "customerDetails"
                )
            )
            // 将信息传递给欺诈检测步骤进行验证
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    fraudDetectionCheckStep,
                    functionName: FraudDetectionStep.Functions.FraudDetectionCheck,
                    parameterName: "customerDetails"
                )
            );

        // 当信用评分检查结果为批准时，信息将被发送到欺诈检测步骤以启动此步骤
        customerCreditCheckStep
            .OnEvent(AccountOpeningEvents.CreditScoreCheckApproved)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    fraudDetectionCheckStep,
                    functionName: FraudDetectionStep.Functions.FraudDetectionCheck,
                    parameterName: "previousCheckSucceeded"
                )
            );

        return process;
    }
}
