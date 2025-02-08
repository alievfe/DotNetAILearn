using System;
using System.Text.Json;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Processes;

/// <summary>
/// 示例流程，展示了如何创建一个具有扇入/扇出行为的流程，以及如何将现有流程作为步骤使用。
/// 该流程用于制作炸鱼薯条，先分别制作炸鱼和薯条，然后添加调味料。
/// 此流程的可视化参考可在 <see href="https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/README.md#fish-and-chips-preparation-process" >流程图</see> 中找到。
/// </summary>
public static class FishAndChipsProcess
{
    public static class ProcessEvents
    {
        // 准备制作炸鱼薯条
        public const string PrepareFishAndChips = nameof(PrepareFishAndChips);

        // 炸鱼薯条已准备好
        public const string FishAndChipsReady = nameof(FishAndChipsReady);

        // 炸鱼薯条的食材缺货
        public const string FishAndChipsIngredientOutOfStock = nameof(
            FishAndChipsIngredientOutOfStock
        );
    }

    /// <summary>
    /// 创建炸鱼薯条制作流程
    /// </summary>
    /// <param name="processName">流程名称，默认为 "FishAndChipsProcess"</param>
    /// <returns>流程构建器实例</returns>
    public static ProcessBuilder CreateProcess(string processName = "FishAndChipsProcess")
    {
        var processBuilder = new ProcessBuilder(processName);
        // 添加制作炸鱼步骤，使用 FriedFishProcess 创建的流程
        var makeFriedFishStep = processBuilder.AddStepFromProcess(FriedFishProcess.CreateProcess());
        // 添加制作薯条步骤，使用 PotatoFriesProcess 创建的流程
        var makePotatoFriesStep = processBuilder.AddStepFromProcess(
            PotatoFriesProcess.CreateProcess()
        );
        // 添加添加调味料步骤
        var addCondimentsStep = processBuilder.AddStepFromType<AddFishAndChipsCondimentsStep>();
        // 添加一个额外步骤，用于确保流程中公共事件名称的唯一性
        var externalStep = processBuilder.AddStepFromType<ExternalFishAndChipsStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFishAndChips)
            .SendEventTo(
                makeFriedFishStep.WhereInputEventIs(FriedFishProcess.ProcessEvents.PrepareFriedFish)
            )
            .SendEventTo(
                makePotatoFriesStep.WhereInputEventIs(
                    PotatoFriesProcess.ProcessEvents.PreparePotatoFries
                )
            );

        makeFriedFishStep
            .OnEvent(FriedFishProcess.ProcessEvents.FriedFishReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(addCondimentsStep, parameterName: "fishActions")
            );

        makePotatoFriesStep
            .OnEvent(PotatoFriesProcess.ProcessEvents.PotatoFriesReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(addCondimentsStep, parameterName: "potatoActions")
            );

        addCondimentsStep
            .OnEvent(AddFishAndChipsCondimentsStep.OutputEvents.CondimentsAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(externalStep));

        return processBuilder;
    }

    /// <summary>
    /// 创建包含有状态步骤的炸鱼薯条制作流程
    /// </summary>
    /// <param name="processName">流程名称，默认为 "FishAndChipsWithStatefulStepsProcess"</param>
    /// <returns>流程构建器实例</returns>
    public static ProcessBuilder CreateProcessWithStatefulSteps(
        string processName = "FishAndChipsWithStatefulStepsProcess"
    )
    {
        var processBuilder = new ProcessBuilder(processName);
        // 添加制作炸鱼步骤，使用 FriedFishProcess 创建的包含有状态步骤的流程版本 1
        var makeFriedFishStep = processBuilder.AddStepFromProcess(
            FriedFishProcess.CreateProcessWithStatefulStepsV1()
        );
        // 添加制作薯条步骤，使用 PotatoFriesProcess 创建的包含有状态步骤的流程
        var makePotatoFriesStep = processBuilder.AddStepFromProcess(
            PotatoFriesProcess.CreateProcessWithStatefulSteps()
        );
        var addCondimentsStep = processBuilder.AddStepFromType<AddFishAndChipsCondimentsStep>();
        var externalStep = processBuilder.AddStepFromType<ExternalFishAndChipsStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFishAndChips)
            .SendEventTo(
                makeFriedFishStep.WhereInputEventIs(FriedFishProcess.ProcessEvents.PrepareFriedFish)
            )
            .SendEventTo(
                makePotatoFriesStep.WhereInputEventIs(
                    PotatoFriesProcess.ProcessEvents.PreparePotatoFries
                )
            );

        makeFriedFishStep
            .OnEvent(FriedFishProcess.ProcessEvents.FriedFishReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(addCondimentsStep, parameterName: "fishActions")
            );

        makePotatoFriesStep
            .OnEvent(PotatoFriesProcess.ProcessEvents.PotatoFriesReady)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(addCondimentsStep, parameterName: "potatoActions")
            );

        addCondimentsStep
            .OnEvent(AddFishAndChipsCondimentsStep.OutputEvents.CondimentsAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(externalStep));

        return processBuilder;
    }

    /// <summary>
    /// 向炸鱼薯条添加调味料的步骤
    /// </summary>
    private sealed class AddFishAndChipsCondimentsStep : KernelProcessStep
    {
        public static class Functions
        {
            // 添加调味料
            public const string AddCondiments = nameof(AddCondiments);
        }

        public static class OutputEvents
        {
            // 调味料已添加
            public const string CondimentsAdded = nameof(CondimentsAdded);
        }

        [KernelFunction(Functions.AddCondiments)]
        public async Task AddCondimentsAsync(
            KernelProcessStepContext context,
            List<string> fishActions,
            List<string> potatoActions
        )
        {
            Console.WriteLine(
                $"ADD_CONDIMENTS: Added condiments to Fish & Chips - Fish: {JsonSerializer.Serialize(fishActions)}, Potatoes: {JsonSerializer.Serialize(potatoActions)}"
            );
            fishActions.AddRange(potatoActions);
            fishActions.Add(FoodIngredients.Condiments.ToFriendlyString());
            await context.EmitEventAsync(
                new() { Id = OutputEvents.CondimentsAdded, Data = fishActions }
            );
        }
    }

    /// <summary>
    /// 外部炸鱼薯条步骤，用于触发炸鱼薯条准备好的事件
    /// </summary>
    private sealed class ExternalFishAndChipsStep : ExternalStep
    {
        public ExternalFishAndChipsStep()
            : base(ProcessEvents.FishAndChipsReady) { }
    }
}
