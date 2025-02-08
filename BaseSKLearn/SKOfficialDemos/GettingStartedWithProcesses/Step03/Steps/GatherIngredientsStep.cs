using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;

/// <summary>
/// 许多其他烹饪流程用作基础的步骤。
/// 此步骤用于收集烹饪所需的食材，在其他流程中使用时，可基于此步骤自定义收集食材的功能。
/// 当用于其他流程时，会基于此步骤创建一个新步骤，并自定义 GatherIngredientsAsync 功能。
/// </summary>
public class GatherIngredientsStep : KernelProcessStep
{
    public static class Functions
    {
        // 收集食材
        public const string GatherIngredients = nameof(GatherIngredients);
    }

    public static class OutputEvents
    {
        // 食材已收集
        public const string IngredientsGathered = nameof(IngredientsGathered);
    }

    private readonly FoodIngredients _ingredient;

    public GatherIngredientsStep(FoodIngredients ingredient)
    {
        this._ingredient = ingredient;
    }

    /// <summary>
    /// 供用户重写的方法，用于设置要收集的自定义食材和要触发的事件。
    /// </summary>
    /// <param name="context">当前步骤和流程的上下文。<see cref="KernelProcessStepContext"/></param>
    /// <param name="foodActions">对食物采取的操作列表</param>
    /// <returns></returns>
    [KernelFunction(Functions.GatherIngredients)]
    public virtual async Task GatherIngredientsAsync(
        KernelProcessStepContext context,
        List<string> foodActions
    )
    {
        var ingredient = this._ingredient.ToFriendlyString();
        var updatedFoodActions = new List<string>();
        updatedFoodActions.AddRange(foodActions);
        if (updatedFoodActions.Count == 0)
        {
            updatedFoodActions.Add(ingredient);
        }
        updatedFoodActions.Add($"{ingredient}_gathered");

        Console.WriteLine($"GATHER_INGREDIENT: Gathered ingredient {ingredient}");
        await context.EmitEventAsync(
            new() { Id = OutputEvents.IngredientsGathered, Data = updatedFoodActions }
        );
    }
}

/// <summary>
/// 许多其他烹饪流程用作基础的有状态步骤。
/// 此步骤在收集食材时会考虑食材库存，当库存不足时会触发相应事件。
/// 当用于其他流程时，会基于此步骤创建一个新步骤，并自定义 GatherIngredientsAsync 功能。
/// </summary>
public class GatherIngredientsWithStockStep : KernelProcessStep<GatherIngredientsState>
{
    public static class Functions
    {
        // 收集食材
        public const string GatherIngredients = nameof(GatherIngredients);
    }

    public static class OutputEvents
    {
        // 食材已收集
        public const string IngredientsGathered = nameof(IngredientsGathered);

        // 食材缺货
        public const string IngredientsOutOfStock = nameof(IngredientsOutOfStock);
    }

    private readonly FoodIngredients _ingredient;

    public GatherIngredientsWithStockStep(FoodIngredients ingredient)
    {
        this._ingredient = ingredient;
    }

    internal GatherIngredientsState? _state;

    public override ValueTask ActivateAsync(KernelProcessStepState<GatherIngredientsState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 供用户重写的方法，用于设置要收集的自定义食材和要触发的事件。
    /// </summary>
    /// <param name="context">当前步骤和流程的上下文。<see cref="KernelProcessStepContext"/></param>
    /// <param name="foodActions">对食物采取的操作列表</param>
    /// <returns></returns>
    [KernelFunction(Functions.GatherIngredients)]
    public virtual async Task GatherIngredientsAsync(
        KernelProcessStepContext context,
        List<string> foodActions
    )
    {
        var ingredient = this._ingredient.ToFriendlyString();
        var updatedFoodActions = new List<string>();
        updatedFoodActions.AddRange(foodActions);

        if (this._state!.IngredientsStock == 0)
        {
            Console.WriteLine($"GATHER_INGREDIENT: Could not gather {ingredient} - OUT OF STOCK!");
            await context.EmitEventAsync(
                new() { Id = OutputEvents.IngredientsOutOfStock, Data = updatedFoodActions }
            );
            return;
        }

        if (updatedFoodActions.Count == 0)
        {
            updatedFoodActions.Add(ingredient);
        }
        updatedFoodActions.Add($"{ingredient}_gathered");

        // 更新食材库存
        this._state.IngredientsStock--;

        Console.WriteLine(
            $"GATHER_INGREDIENT: Gathered ingredient {ingredient} - remaining: {this._state.IngredientsStock}"
        );
        await context.EmitEventAsync(
            new() { Id = OutputEvents.IngredientsGathered, Data = updatedFoodActions }
        );
    }
}

/// <summary>
/// <see cref="GatherIngredientsWithStockStep"/> 的状态对象，用于记录食材的库存数量。
/// </summary>
public sealed class GatherIngredientsState
{
    public int IngredientsStock { get; set; } = 5;
}
