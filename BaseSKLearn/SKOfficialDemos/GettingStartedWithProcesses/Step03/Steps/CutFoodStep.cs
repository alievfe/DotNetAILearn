namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process;

/// <summary>
/// 流程示例中使用的步骤：
/// - Step_03_FoodPreparation.cs
/// 此步骤用于模拟烹饪流程中切食物的操作，提供了切碎和切片两种切法。
/// </summary>
[KernelProcessStepMetadata("CutFoodStep.V1")]
public class CutFoodStep : KernelProcessStep
{
    public static class Functions
    {
        // 切碎食物
        public const string ChopFood = nameof(ChopFood);

        // 切片食物
        public const string SliceFood = nameof(SliceFood);
    }

    public static class OutputEvents
    {
        // 切碎操作完成
        public const string ChoppingReady = nameof(ChoppingReady);

        // 切片操作完成
        public const string SlicingReady = nameof(SlicingReady);
    }

    [KernelFunction(Functions.ChopFood)]
    public async Task ChopFoodAsync(KernelProcessStepContext context, List<string> foodActions)
    {
        // 获取要切的食物
        var foodToBeCut = foodActions.First();
        // 将切碎操作结果添加到食物操作列表中
        foodActions.Add(this.getActionString(foodToBeCut, "chopped"));
        Console.WriteLine($"CUTTING_STEP: Ingredient {foodToBeCut} has been chopped!");
        // 触发切碎完成事件
        await context.EmitEventAsync(new() { Id = OutputEvents.ChoppingReady, Data = foodActions });
    }

    [KernelFunction(Functions.SliceFood)]
    public async Task SliceFoodAsync(KernelProcessStepContext context, List<string> foodActions)
    {
        // 获取要切的食物
        var foodToBeCut = foodActions.First();
        // 将切片操作结果添加到食物操作列表中
        foodActions.Add(this.getActionString(foodToBeCut, "sliced"));
        Console.WriteLine($"CUTTING_STEP: Ingredient {foodToBeCut} has been sliced!");
        // 触发切片完成事件
        await context.EmitEventAsync(new() { Id = OutputEvents.SlicingReady, Data = foodActions });
    }

    // 生成表示食物操作结果的字符串
    private string getActionString(string food, string action)
    {
        return $"{food}_{action}";
    }
}
