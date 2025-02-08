using System.Text.Json;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Processes;

/// <summary>
/// 示例流程，展示了如何创建一个选择性扇出流程。
/// 关于此流程的可视化参考，请查看 <see href="https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/README.md#single-order-preparation-process" >流程图</see>
/// </summary>
public static class SingleFoodItemProcess
{
    public static class ProcessEvents
    {
        // 收到单个订单
        public const string SingleOrderReceived = nameof(SingleOrderReceived);

        // 单个订单已准备好
        public const string SingleOrderReady = nameof(SingleOrderReady);
    }

    /// <summary>
    /// 创建单个食物项处理流程
    /// </summary>
    /// <param name="processName">流程名称，默认为 "SingleFoodItemProcess"</param>
    /// <returns>流程构建器实例</returns>
    public static ProcessBuilder CreateProcess(string processName = "SingleFoodItemProcess")
    {
        var processBuilder = new ProcessBuilder(processName);

        // 添加订单分配步骤
        var dispatchOrderStep = processBuilder.AddStepFromType<DispatchSingleOrderStep>();
        // 添加制作炸鱼步骤，使用 FriedFishProcess 创建的流程
        var makeFriedFishStep = processBuilder.AddStepFromProcess(FriedFishProcess.CreateProcess());
        // 添加制作薯条步骤，使用 PotatoFriesProcess 创建的流程
        var makePotatoFriesStep = processBuilder.AddStepFromProcess(
            PotatoFriesProcess.CreateProcess()
        );
        // 添加制作鱼三明治步骤，使用 FishSandwichProcess 创建的流程
        var makeFishSandwichStep = processBuilder.AddStepFromProcess(
            FishSandwichProcess.CreateProcess()
        );
        // 添加制作炸鱼薯条步骤，使用 FishAndChipsProcess 创建的流程
        var makeFishAndChipsStep = processBuilder.AddStepFromProcess(
            FishAndChipsProcess.CreateProcess()
        );
        // 添加打包订单步骤
        var packOrderStep = processBuilder.AddStepFromType<PackOrderStep>();
        // 添加外部步骤，用于确保流程中公共事件名称的唯一性
        var externalStep = processBuilder.AddStepFromType<ExternalSingleOrderStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.SingleOrderReceived)
            .SendEventTo(new ProcessFunctionTargetBuilder(dispatchOrderStep));

        dispatchOrderStep
            .OnEvent(DispatchSingleOrderStep.OutputEvents.PrepareFriedFish)
            .SendEventTo(
                makeFriedFishStep.WhereInputEventIs(FriedFishProcess.ProcessEvents.PrepareFriedFish)
            );

        dispatchOrderStep
            .OnEvent(DispatchSingleOrderStep.OutputEvents.PrepareFries)
            .SendEventTo(
                makePotatoFriesStep.WhereInputEventIs(
                    PotatoFriesProcess.ProcessEvents.PreparePotatoFries
                )
            );

        dispatchOrderStep
            .OnEvent(DispatchSingleOrderStep.OutputEvents.PrepareFishSandwich)
            .SendEventTo(
                makeFishSandwichStep.WhereInputEventIs(
                    FishSandwichProcess.ProcessEvents.PrepareFishSandwich
                )
            );

        dispatchOrderStep
            .OnEvent(DispatchSingleOrderStep.OutputEvents.PrepareFishAndChips)
            .SendEventTo(
                makeFishAndChipsStep.WhereInputEventIs(
                    FishAndChipsProcess.ProcessEvents.PrepareFishAndChips
                )
            );

        makeFriedFishStep
            .OnEvent(FriedFishProcess.ProcessEvents.FriedFishReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(packOrderStep));

        makePotatoFriesStep
            .OnEvent(PotatoFriesProcess.ProcessEvents.PotatoFriesReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(packOrderStep));

        makeFishSandwichStep
            .OnEvent(FishSandwichProcess.ProcessEvents.FishSandwichReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(packOrderStep));

        makeFishAndChipsStep
            .OnEvent(FishAndChipsProcess.ProcessEvents.FishAndChipsReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(packOrderStep));

        packOrderStep
            .OnEvent(PackOrderStep.OutputEvents.FoodPacked)
            .SendEventTo(new ProcessFunctionTargetBuilder(externalStep));

        return processBuilder;
    }

    /// <summary>
    /// 分配单个订单的步骤
    /// </summary>
    private sealed class DispatchSingleOrderStep : KernelProcessStep
    {
        public static class Functions
        {
            // 准备单个订单
            public const string PrepareSingleOrder = nameof(PrepareSingleOrder);
        }

        public static class OutputEvents
        {
            // 准备薯条
            public const string PrepareFries = nameof(PrepareFries);

            // 准备炸鱼
            public const string PrepareFriedFish = nameof(PrepareFriedFish);

            // 准备鱼三明治
            public const string PrepareFishSandwich = nameof(PrepareFishSandwich);

            // 准备炸鱼薯条
            public const string PrepareFishAndChips = nameof(PrepareFishAndChips);
        }

        [KernelFunction(Functions.PrepareSingleOrder)]
        public async Task DispatchSingleOrderAsync(
            KernelProcessStepContext context,
            FoodItem foodItem
        )
        {
            // 获取食物的友好名称
            var foodName = foodItem.ToFriendlyString();
            Console.WriteLine($"DISPATCH_SINGLE_ORDER: Dispatching '{foodName}'!");
            var foodActions = new List<string>();

            switch (foodItem)
            {
                case FoodItem.PotatoFries:
                    await context.EmitEventAsync(
                        new() { Id = OutputEvents.PrepareFries, Data = foodActions }
                    );
                    break;
                case FoodItem.FriedFish:
                    await context.EmitEventAsync(
                        new() { Id = OutputEvents.PrepareFriedFish, Data = foodActions }
                    );
                    break;
                case FoodItem.FishSandwich:
                    await context.EmitEventAsync(
                        new() { Id = OutputEvents.PrepareFishSandwich, Data = foodActions }
                    );
                    break;
                case FoodItem.FishAndChips:
                    await context.EmitEventAsync(
                        new() { Id = OutputEvents.PrepareFishAndChips, Data = foodActions }
                    );
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 打包订单的步骤
    /// </summary>
    private sealed class PackOrderStep : KernelProcessStep
    {
        public static class Functions
        {
            // 打包食物
            public const string PackFood = nameof(PackFood);
        }

        public static class OutputEvents
        {
            // 食物已打包
            public const string FoodPacked = nameof(FoodPacked);
        }

        [KernelFunction(Functions.PackFood)]
        public async Task PackFoodAsync(KernelProcessStepContext context, List<string> foodActions)
        {
            Console.WriteLine(
                $"PACKING_FOOD: Food {foodActions.First()} Packed! - {JsonSerializer.Serialize(foodActions)}"
            );
            await context.EmitEventAsync(new() { Id = OutputEvents.FoodPacked });
        }
    }

    /// <summary>
    /// 外部单个订单步骤，用于触发单个订单准备好的事件
    /// </summary>
    private sealed class ExternalSingleOrderStep : ExternalStep
    {
        public ExternalSingleOrderStep()
            : base(ProcessEvents.SingleOrderReady) { }
    }
}
