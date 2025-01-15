using System;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step00.Steps;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step00;

/// <summary>
/// 演示如何创建最简单的 <see cref="KernelProcess"/> 并获取其对三条明确用户消息的响应。
/// </summary>
public class Step00_Processes
{
    public static class ProcessEvents
    {
        public const string StartProcess = nameof(StartProcess);
    }

    /// <summary>
    /// 演示如何创建一个包含多个步骤的最简单流程。
    /// </summary>
    /// <returns>返回一个 <see cref="Task"/>。</returns>
    public async Task UseSimplestProcessAsync()
    {
        Kernel kernel = Kernel.CreateBuilder().Build();

        // 创建一个将与聊天完成服务交互的流程
        ProcessBuilder process = new("ChatBot");
        var startStep = process.AddStepFromType<StartStep>();
        var doSomeWorkStep = process.AddStepFromType<DoSomeWorkStep>();
        var doMoreWorkStep = process.AddStepFromType<DoMoreWorkStep>();
        var lastStep = process.AddStepFromType<LastStep>();

        // 定义流程的流转逻辑
        process
            .OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(startStep));
        startStep.OnFunctionResult().SendEventTo(new ProcessFunctionTargetBuilder(doSomeWorkStep));
        doSomeWorkStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(doMoreWorkStep));
        doMoreWorkStep.OnFunctionResult().SendEventTo(new ProcessFunctionTargetBuilder(lastStep));
        lastStep.OnFunctionResult().StopProcess();

        // 构建流程以获取可以启动的句柄
        KernelProcess kernelProcess = process.Build();

        // 使用初始外部事件启动流程
        using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = ProcessEvents.StartProcess, Data = null }
        );
    }
}
/*
Step 1 - Start

Step 2 - Doing Some Work...

Step 3 - Doing Yet More Work...

Step 4 - This is the Final Step...
*/