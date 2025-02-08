namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process;

/// <summary>
/// 流程示例中使用的步骤：
/// - Step_03_FoodPreparation.cs
/// 此步骤在切食物时考虑刀具锋利度，若刀具不够锋利则需先磨刀。
/// </summary>
[KernelProcessStepMetadata("CutFoodStep.V2")]
public class CutFoodWithSharpeningStep : KernelProcessStep<CutFoodWithSharpeningState>
{
    public static class Functions
    {
        // 切碎食物
        public const string ChopFood = nameof(ChopFood);

        // 切片食物
        public const string SliceFood = nameof(SliceFood);

        // 磨刀
        public const string SharpenKnife = nameof(SharpenKnife);
    }

    public static class OutputEvents
    {
        // 切碎操作完成
        public const string ChoppingReady = nameof(ChoppingReady);

        // 切片操作完成
        public const string SlicingReady = nameof(SlicingReady);

        // 刀具需要磨刀
        public const string KnifeNeedsSharpening = nameof(KnifeNeedsSharpening);

        // 刀具已磨好
        public const string KnifeSharpened = nameof(KnifeSharpened);
    }

    internal CutFoodWithSharpeningState? _state;

    public override ValueTask ActivateAsync(
        KernelProcessStepState<CutFoodWithSharpeningState> state
    )
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    [KernelFunction(Functions.ChopFood)]
    public async Task ChopFoodAsync(KernelProcessStepContext context, List<string> foodActions)
    {
        var foodToBeCut = foodActions.First();
        if (this.KnifeNeedsSharpening())
        {
            Console.WriteLine(
                $"CUTTING_STEP: Dull knife, cannot chop {foodToBeCut} - needs sharpening."
            );
            await context.EmitEventAsync(
                new() { Id = OutputEvents.KnifeNeedsSharpening, Data = foodActions }
            );
            return;
        }
        // 更新刀具锋利度
        this._state!.KnifeSharpness--;

        // 切碎食物
        foodActions.Add(this.getActionString(foodToBeCut, "chopped"));
        Console.WriteLine(
            $"CUTTING_STEP: Ingredient {foodToBeCut} has been chopped! - knife sharpness: {this._state.KnifeSharpness}"
        );
        await context.EmitEventAsync(new() { Id = OutputEvents.ChoppingReady, Data = foodActions });
    }

    [KernelFunction(Functions.SliceFood)]
    public async Task SliceFoodAsync(KernelProcessStepContext context, List<string> foodActions)
    {
        var foodToBeCut = foodActions.First();
        if (this.KnifeNeedsSharpening())
        {
            Console.WriteLine(
                $"CUTTING_STEP: Dull knife, cannot slice {foodToBeCut} - needs sharpening."
            );
            await context.EmitEventAsync(
                new() { Id = OutputEvents.KnifeNeedsSharpening, Data = foodActions }
            );
            return;
        }
        // 更新刀具锋利度
        this._state!.KnifeSharpness--;

        // 切片食物
        foodActions.Add(this.getActionString(foodToBeCut, "sliced"));
        Console.WriteLine(
            $"CUTTING_STEP: Ingredient {foodToBeCut} has been sliced! - knife sharpness: {this._state.KnifeSharpness}"
        );
        await context.EmitEventAsync(new() { Id = OutputEvents.SlicingReady, Data = foodActions });
    }

    [KernelFunction(Functions.SharpenKnife)]
    public async Task SharpenKnifeAsync(KernelProcessStepContext context, List<string> foodActions)
    {
        this._state!.KnifeSharpness += this._state._sharpeningBoost;
        Console.WriteLine($"KNIFE SHARPENED: Knife sharpness is now {this._state.KnifeSharpness}!");
        await context.EmitEventAsync(
            new() { Id = OutputEvents.KnifeSharpened, Data = foodActions }
        );
    }

    private bool KnifeNeedsSharpening() =>
        this._state?.KnifeSharpness == this._state?._needsSharpeningLimit;

    private string getActionString(string food, string action)
    {
        return $"{food}_{action}";
    }
}

/// <summary>
/// <see cref="CutFoodWithSharpeningStep"/> 的状态对象，用于记录刀具的锋利度、磨刀界限和磨刀提升值。
/// </summary>
public sealed class CutFoodWithSharpeningState
{
    // 刀具锋利度
    public int KnifeSharpness { get; set; } = 5;

    // 需要磨刀的界限
    internal int _needsSharpeningLimit = 3;

    // 磨刀提升值
    internal int _sharpeningBoost = 5;
}
