using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;

/// <summary>
/// 炸食物步骤
/// 流程示例中使用的步骤：
/// - Step_03_FoodPreparation.cs
/// </summary>
[KernelProcessStepMetadata("FryFoodStep.V1")]
public class FryFoodStep : KernelProcessStep
{
    public static class Functions
    {
        // 炸食物
        public const string FryFood = nameof(FryFood);
    }

    public static class OutputEvents
    {
        // 食物炸坏了
        public const string FoodRuined = nameof(FoodRuined);

        // 炸好的食物已就绪
        public const string FriedFoodReady = nameof(FriedFoodReady);
    }

    private readonly Random _randomSeed = new();

    [KernelFunction(Functions.FryFood)]
    public async Task FryFoodAsync(KernelProcessStepContext context, List<string> foodActions)
    {
        // 获取要炸的食物
        var foodToFry = foodActions.First();
        // 这个步骤有时可能会失败
        int fryerMalfunction = _randomSeed.Next(0, 10);

        // 可以潜在地使用 foodToFry 来设置油炸温度和烹饪时长
        if (fryerMalfunction < 5)
        {
            // 哦不！食物炸糊了 :(
            foodActions.Add($"{foodToFry}_frying_failed");
            Console.WriteLine($"FRYING_STEP: Ingredient {foodToFry} got burnt while frying :(");
            await context.EmitEventAsync(
                new() { Id = OutputEvents.FoodRuined, Data = foodActions }
            );
            return;
        }

        foodActions.Add($"{foodToFry}_frying_succeeded");
        Console.WriteLine($"FRYING_STEP: Ingredient {foodToFry} is ready!");
        await context.EmitEventAsync(
            new()
            {
                Id = OutputEvents.FriedFoodReady,
                Data = foodActions,
                Visibility = KernelProcessEventVisibility.Public,
            }
        );
    }
}
