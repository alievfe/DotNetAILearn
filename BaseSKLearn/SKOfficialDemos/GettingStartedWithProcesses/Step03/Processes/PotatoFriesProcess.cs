using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Processes;

// 版权所有 (c) 微软。保留所有权利。



/// <summary>
/// 示例流程，展示了如何创建一个包含顺序步骤的流程以及如何复用现有步骤。
/// 该流程用于模拟制作薯条的过程，包含收集食材、切片和油炸等步骤。
/// </summary>
public static class PotatoFriesProcess
{
    public static class ProcessEvents
    {
        // 准备制作薯条
        public const string PreparePotatoFries = nameof(PreparePotatoFries);

        // 当多个流程使用相同的最终步骤时，该步骤的事件应标记为公共事件
        // 这样该步骤的事件也可以用作流程的输出事件。
        // 在这些示例中，炸鱼和薯条都以 FryStep 成功结束
        public const string PotatoFriesReady = nameof(FryFoodStep.OutputEvents.FriedFoodReady);
    }

    /// <summary>
    /// 有关薯条制作流程的可视化参考，请查看此
    /// <see href="https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/README.md#potato-fries-preparation-process" >流程图</see>
    /// </summary>
    /// <param name="processName">流程名称</param>
    /// <returns><see cref="ProcessBuilder"/></returns>
    public static ProcessBuilder CreateProcess(string processName = "PotatoFriesProcess")
    {
        var processBuilder = new ProcessBuilder(processName);

        var gatherIngredientsStep =
            processBuilder.AddStepFromType<GatherPotatoFriesIngredientsStep>();
        var sliceStep = processBuilder.AddStepFromType<CutFoodStep>("sliceStep");
        var fryStep = processBuilder.AddStepFromType<FryFoodStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PreparePotatoFries)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        gatherIngredientsStep
            .OnEvent(GatherPotatoFriesIngredientsStep.OutputEvents.IngredientsGathered)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    sliceStep,
                    functionName: CutFoodStep.Functions.SliceFood
                )
            );

        sliceStep
            .OnEvent(CutFoodStep.OutputEvents.SlicingReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(fryStep));

        fryStep
            .OnEvent(FryFoodStep.OutputEvents.FoodRuined)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        return processBuilder;
    }

    /// <summary>
    /// 有关包含有状态步骤的薯条制作流程的可视化参考，请查看此
    /// <see href="https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/README.md#potato-fries-preparation-with-knife-sharpening-and-ingredient-stock-process" >流程图</see>
    /// </summary>
    /// <param name="processName">流程名称</param>
    /// <returns><see cref="ProcessBuilder"/></returns>
    public static ProcessBuilder CreateProcessWithStatefulSteps(
        string processName = "PotatoFriesWithStatefulStepsProcess"
    )
    {
        var processBuilder = new ProcessBuilder(processName);

        var gatherIngredientsStep =
            processBuilder.AddStepFromType<GatherPotatoFriesIngredientsWithStockStep>();
        var sliceStep = processBuilder.AddStepFromType<CutFoodWithSharpeningStep>("sliceStep");
        var fryStep = processBuilder.AddStepFromType<FryFoodStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PreparePotatoFries)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        gatherIngredientsStep
            .OnEvent(GatherPotatoFriesIngredientsWithStockStep.OutputEvents.IngredientsGathered)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    sliceStep,
                    functionName: CutFoodWithSharpeningStep.Functions.SliceFood
                )
            );

        gatherIngredientsStep
            .OnEvent(GatherPotatoFriesIngredientsWithStockStep.OutputEvents.IngredientsOutOfStock)
            .StopProcess();

        sliceStep
            .OnEvent(CutFoodWithSharpeningStep.OutputEvents.SlicingReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(fryStep));

        sliceStep
            .OnEvent(CutFoodWithSharpeningStep.OutputEvents.KnifeNeedsSharpening)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    sliceStep,
                    functionName: CutFoodWithSharpeningStep.Functions.SharpenKnife
                )
            );

        sliceStep
            .OnEvent(CutFoodWithSharpeningStep.OutputEvents.KnifeSharpened)
            .SendEventTo(
                new ProcessFunctionTargetBuilder(
                    sliceStep,
                    functionName: CutFoodWithSharpeningStep.Functions.SliceFood
                )
            );

        fryStep
            .OnEvent(FryFoodStep.OutputEvents.FoodRuined)
            .SendEventTo(new ProcessFunctionTargetBuilder(gatherIngredientsStep));

        return processBuilder;
    }

    private sealed class GatherPotatoFriesIngredientsStep : GatherIngredientsStep
    {
        public GatherPotatoFriesIngredientsStep()
            : base(FoodIngredients.Pototoes) { }
    }

    private sealed class GatherPotatoFriesIngredientsWithStockStep : GatherIngredientsWithStockStep
    {
        public GatherPotatoFriesIngredientsWithStockStep()
            : base(FoodIngredients.Pototoes) { }
    }
}
