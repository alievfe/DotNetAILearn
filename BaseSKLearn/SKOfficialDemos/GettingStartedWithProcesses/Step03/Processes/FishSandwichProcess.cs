using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Steps;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Processes;

/// <summary>
/// 示例流程，展示了如何创建一个包含顺序步骤的流程，以及如何将现有流程作为步骤使用。
/// 此流程用于制作鱼三明治，包含制作炸鱼、添加面包、添加特制酱料等步骤。
/// 该流程的可视化参考可在 <see href="https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/GettingStartedWithProcesses/README.md#fish-sandwich-preparation-process" >流程图</see> 中找到。
/// </summary>
public static class FishSandwichProcess
{
    public static class ProcessEvents
    {
        // 准备制作鱼三明治
        public const string PrepareFishSandwich = nameof(PrepareFishSandwich);

        // 鱼三明治已准备好
        public const string FishSandwichReady = nameof(FishSandwichReady);
    }

    /// <summary>
    /// 创建鱼三明治制作流程
    /// </summary>
    /// <param name="processName">流程名称，默认为 "FishSandwichProcess"</param>
    /// <returns>流程构建器实例</returns>
    public static ProcessBuilder CreateProcess(string processName = "FishSandwichProcess")
    {
        var processBuilder = new ProcessBuilder(processName);
        // 添加制作炸鱼步骤，使用 FriedFishProcess 创建的流程
        var makeFriedFishStep = processBuilder.AddStepFromProcess(FriedFishProcess.CreateProcess());
        // 添加添加面包步骤
        var addBunsStep = processBuilder.AddStepFromType<AddBunsStep>();
        // 添加添加特制酱料步骤
        var addSpecialSauceStep = processBuilder.AddStepFromType<AddSpecialSauceStep>();
        // 添加一个额外步骤，用于确保流程中公共事件名称的唯一性
        var externalStep = processBuilder.AddStepFromType<ExternalFriedFishStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFishSandwich)
            .SendEventTo(
                makeFriedFishStep.WhereInputEventIs(FriedFishProcess.ProcessEvents.PrepareFriedFish)
            );

        makeFriedFishStep
            .OnEvent(FriedFishProcess.ProcessEvents.FriedFishReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(addBunsStep));

        addBunsStep
            .OnEvent(AddBunsStep.OutputEvents.BunsAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(addSpecialSauceStep));

        addSpecialSauceStep
            .OnEvent(AddSpecialSauceStep.OutputEvents.SpecialSauceAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(externalStep));

        return processBuilder;
    }

    /// <summary>
    /// 创建包含有状态步骤的鱼三明治制作流程版本 1
    /// </summary>
    /// <param name="processName">流程名称，默认为 "FishSandwichWithStatefulStepsProcess"</param>
    /// <returns>流程构建器实例</returns>
    public static ProcessBuilder CreateProcessWithStatefulStepsV1(
        string processName = "FishSandwichWithStatefulStepsProcess"
    )
    {
        var processBuilder = new ProcessBuilder(processName) { Version = "FishSandwich.V1" };
        // 添加制作炸鱼步骤，使用 FriedFishProcess 创建的包含有状态步骤的流程版本 1
        var makeFriedFishStep = processBuilder.AddStepFromProcess(
            FriedFishProcess.CreateProcessWithStatefulStepsV1()
        );
        var addBunsStep = processBuilder.AddStepFromType<AddBunsStep>();
        var addSpecialSauceStep = processBuilder.AddStepFromType<AddSpecialSauceStep>();
        var externalStep = processBuilder.AddStepFromType<ExternalFriedFishStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFishSandwich)
            .SendEventTo(
                makeFriedFishStep.WhereInputEventIs(FriedFishProcess.ProcessEvents.PrepareFriedFish)
            );

        makeFriedFishStep
            .OnEvent(FriedFishProcess.ProcessEvents.FriedFishReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(addBunsStep));

        addBunsStep
            .OnEvent(AddBunsStep.OutputEvents.BunsAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(addSpecialSauceStep));

        addSpecialSauceStep
            .OnEvent(AddSpecialSauceStep.OutputEvents.SpecialSauceAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(externalStep));

        return processBuilder;
    }

    /// <summary>
    /// 创建包含有状态步骤的鱼三明治制作流程版本 2
    /// </summary>
    /// <param name="processName">流程名称，默认为 "FishSandwichWithStatefulStepsProcess"</param>
    /// <returns>流程构建器实例</returns>
    public static ProcessBuilder CreateProcessWithStatefulStepsV2(
        string processName = "FishSandwichWithStatefulStepsProcess"
    )
    {
        var processBuilder = new ProcessBuilder(processName) { Version = "FishSandwich.V2" };
        // 添加制作炸鱼步骤，使用 FriedFishProcess 创建的包含有状态步骤的流程版本 2
        var makeFriedFishStep = processBuilder.AddStepFromProcess(
            FriedFishProcess.CreateProcessWithStatefulStepsV2("FriedFishStep"),
            aliases: ["FriedFishWithStatefulStepsProcess"]
        );
        var addBunsStep = processBuilder.AddStepFromType<AddBunsStep>();
        var addSpecialSauceStep = processBuilder.AddStepFromType<AddSpecialSauceStep>();
        var externalStep = processBuilder.AddStepFromType<ExternalFriedFishStep>();

        processBuilder
            .OnInputEvent(ProcessEvents.PrepareFishSandwich)
            .SendEventTo(
                makeFriedFishStep.WhereInputEventIs(FriedFishProcess.ProcessEvents.PrepareFriedFish)
            );

        makeFriedFishStep
            .OnEvent(FriedFishProcess.ProcessEvents.FriedFishReady)
            .SendEventTo(new ProcessFunctionTargetBuilder(addBunsStep));

        addBunsStep
            .OnEvent(AddBunsStep.OutputEvents.BunsAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(addSpecialSauceStep));

        addSpecialSauceStep
            .OnEvent(AddSpecialSauceStep.OutputEvents.SpecialSauceAdded)
            .SendEventTo(new ProcessFunctionTargetBuilder(externalStep));

        return processBuilder;
    }

    /// <summary>
    /// 向食物中添加面包的步骤
    /// </summary>
    private sealed class AddBunsStep : KernelProcessStep
    {
        public static class Functions
        {
            // 添加面包
            public const string AddBuns = nameof(AddBuns);
        }

        public static class OutputEvents
        {
            // 面包已添加
            public const string BunsAdded = nameof(BunsAdded);
        }

        [KernelFunction(Functions.AddBuns)]
        public async Task SliceFoodAsync(KernelProcessStepContext context, List<string> foodActions)
        {
            Console.WriteLine($"BUNS_ADDED_STEP: Buns added to ingredient {foodActions.First()}");
            foodActions.Add(FoodIngredients.Buns.ToFriendlyString());
            await context.EmitEventAsync(new() { Id = OutputEvents.BunsAdded, Data = foodActions });
        }
    }

    /// <summary>
    /// 向食物中添加特制酱料的步骤
    /// </summary>
    private sealed class AddSpecialSauceStep : KernelProcessStep
    {
        public static class Functions
        {
            // 添加特制酱料
            public const string AddSpecialSauce = nameof(AddSpecialSauce);
        }

        public static class OutputEvents
        {
            // 特制酱料已添加
            public const string SpecialSauceAdded = nameof(SpecialSauceAdded);
        }

        [KernelFunction(Functions.AddSpecialSauce)]
        public async Task SliceFoodAsync(KernelProcessStepContext context, List<string> foodActions)
        {
            Console.WriteLine(
                $"SPECIAL_SAUCE_ADDED: Special sauce added to ingredient {foodActions.First()}"
            );
            foodActions.Add(FoodIngredients.Sauce.ToFriendlyString());
            await context.EmitEventAsync(
                new()
                {
                    Id = OutputEvents.SpecialSauceAdded,
                    Data = foodActions,
                    Visibility = KernelProcessEventVisibility.Public,
                }
            );
        }
    }

    /// <summary>
    /// 外部炸鱼步骤，用于触发鱼三明治准备好的事件
    /// </summary>
    private sealed class ExternalFriedFishStep : ExternalStep
    {
        public ExternalFriedFishStep()
            : base(ProcessEvents.FishSandwichReady) { }
    }
}
