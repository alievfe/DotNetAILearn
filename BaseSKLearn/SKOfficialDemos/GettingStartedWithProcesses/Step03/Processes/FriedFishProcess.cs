using System;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Processes;

/// <summary>
/// 示例流程，展示了如何创建一个包含顺序步骤的流程以及如何复用现有步骤。<br/>
/// </summary>
public static class FriedFishProcess
{
    public static class ProcessEvents
    {
        // 准备炸鱼
        public const string PrepareFriedFish = nameof(PrepareFriedFish);

        // 当多个流程使用相同的最终步骤时，该步骤的事件应标记为公共事件
        // 这样该步骤的事件也可以用作流程的输出事件。
        // 在这些示例中，炸鱼和炸薯条都以 FryStep 成功结束
        public const string FriedFishReady = FryFoodStep.OutputEvents.FriedFoodReady;
    }

    /// <summary>
    /// 有关炸鱼流程的可视化参考，请查看此
    /// <see href="https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/README.md#fried-fish-preparation-process" >流程图</see>
    /// </summary>
    /// <param name="processName">流程名称</param>
    /// <returns><see cref="ProcessBuilder"/></returns>
    public static ProcessBuilder CreateProcess(string processName = "FriedFishProcess")
    {
        var processBuilder = new ProcessBuilder(processName);

        var gatherIngredientsStep =
            processBuilder.AddStepFromType<GatherFriedFishIngredientsStep>();
        var chopStep = processBuilder.AddStepFromType<CutFoodStep>();
        var fryStep = processBuilder.AddStepFromType<FryFoodStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFriedFish)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        gatherIngredientsStep
            .OnEvent(GatherFriedFishIngredientsStep.OutputEvents.IngredientsGathered)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    chopStep,
                    functionName: CutFoodStep.Functions.ChopFood
                )
            );

        chopStep
            .OnEvent(CutFoodStep.OutputEvents.ChoppingReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(fryStep));

        fryStep
            .OnEvent(FryFoodStep.OutputEvents.FoodRuined)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        return processBuilder;
    }

    public static ProcessBuilder CreateProcessWithStatefulStepsV1(
        string processName = "FriedFishWithStatefulStepsProcess"
    )
    {
        // 建议指定流程版本，以防此流程被其他流程用作一个步骤
        var processBuilder = new ProcessBuilder(processName) { Version = "FriedFishProcess.v1" };

        var gatherIngredientsStep =
            processBuilder.AddStepFromType<GatherFriedFishIngredientsWithStockStep>();
        var chopStep = processBuilder.AddStepFromType<CutFoodStep>();
        var fryStep = processBuilder.AddStepFromType<FryFoodStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFriedFish)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        gatherIngredientsStep
            .OnEvent(GatherFriedFishIngredientsWithStockStep.OutputEvents.IngredientsGathered)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    chopStep,
                    functionName: CutFoodWithSharpeningStep.Functions.ChopFood
                )
            );

        chopStep
            .OnEvent(CutFoodWithSharpeningStep.OutputEvents.ChoppingReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(fryStep));

        fryStep
            .OnEvent(FryFoodStep.OutputEvents.FoodRuined)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        return processBuilder;
    }

    /// <summary>
    /// 有关包含有状态步骤的炸鱼流程的可视化参考，请查看此
    /// <see href="https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/README.md#fried-fish-preparation-with-knife-sharpening-and-ingredient-stock-process" >流程图</see>
    /// </summary>
    /// <param name="processName">流程名称</param>
    /// <returns><see cref="ProcessBuilder"/></returns>
    public static ProcessBuilder CreateProcessWithStatefulStepsV2(
        string processName = "FriedFishWithStatefulStepsProcess"
    )
    {
        // 建议指定流程版本，以防此流程被其他流程用作一个步骤
        var processBuilder = new ProcessBuilder(processName) { Version = "FriedFishProcess.v2" };

        // aliases可设置以前版本的步骤使用过的别名，用于在读取旧版本的进程状态时支持向后兼容性
        var gatherIngredientsStep =
            processBuilder.AddStepFromType<GatherFriedFishIngredientsWithStockStep>(
                name: "gatherFishIngredientStep",
                aliases: ["GatherFriedFishIngredientsWithStockStep"]
            );
        var chopStep = processBuilder.AddStepFromType<CutFoodWithSharpeningStep>(
            name: "chopFishStep",
            aliases: ["CutFoodStep"]
        );
        var fryStep = processBuilder.AddStepFromType<FryFoodStep>(
            name: "fryFishStep",
            aliases: ["FryFoodStep"]
        );

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFriedFish)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        gatherIngredientsStep
            .OnEvent(GatherFriedFishIngredientsWithStockStep.OutputEvents.IngredientsGathered)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    chopStep,
                    functionName: CutFoodWithSharpeningStep.Functions.ChopFood
                )
            );

        gatherIngredientsStep
            .OnEvent(GatherFriedFishIngredientsWithStockStep.OutputEvents.IngredientsOutOfStock)
            .StopProcess();

        chopStep
            .OnEvent(CutFoodWithSharpeningStep.OutputEvents.ChoppingReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(fryStep));

        chopStep
            .OnEvent(CutFoodWithSharpeningStep.OutputEvents.KnifeNeedsSharpening)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    chopStep,
                    functionName: CutFoodWithSharpeningStep.Functions.SharpenKnife
                )
            );

        chopStep
            .OnEvent(CutFoodWithSharpeningStep.OutputEvents.KnifeSharpened)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    chopStep,
                    functionName: CutFoodWithSharpeningStep.Functions.ChopFood
                )
            );

        fryStep
            .OnEvent(FryFoodStep.OutputEvents.FoodRuined)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        return processBuilder;
    }

    [KernelProcessStepMetadata("GatherFishIngredient.V1")]
    private sealed class GatherFriedFishIngredientsStep : GatherIngredientsStep
    {
        public GatherFriedFishIngredientsStep()
            : base(FoodIngredients.Fish) { }
    }

    [KernelProcessStepMetadata("GatherFishIngredient.V2")]
    private sealed class GatherFriedFishIngredientsWithStockStep : GatherIngredientsWithStockStep
    {
        public GatherFriedFishIngredientsWithStockStep()
            : base(FoodIngredients.Fish) { }
    }
}
