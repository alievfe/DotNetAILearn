using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Models;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Processes;
using Microsoft.SemanticKernel;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03;

/// <summary>
/// 展示子流程作为步骤的使用，生成多个事件，有条件地重用食品制备样本。
/// 演示如何创建 <see cref="KernelProcess"/> 并触发不同的与食物相关的事件。
/// 关于此处使用的流程的可视化参考，请查看以下文档中的图表：https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/README.md#step03b_foodOrdering
/// </summary>
public class Step03b_FoodOrdering
{
    /// <summary>
    /// 测试单个炸鱼订单的处理流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UseSingleOrderFriedFishAsync()
    {
        // 调用处理单个食物订单的方法，传入炸鱼食物项
        await UsePrepareFoodOrderProcessSingleItemAsync(FoodItem.FriedFish);
    }

    /// <summary>
    /// 测试单个薯条订单的处理流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UseSingleOrderPotatoFriesAsync()
    {
        // 调用处理单个食物订单的方法，传入薯条食物项
        await UsePrepareFoodOrderProcessSingleItemAsync(FoodItem.PotatoFries);
    }

    /// <summary>
    /// 测试单个鱼三明治订单的处理流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UseSingleOrderFishSandwichAsync()
    {
        // 调用处理单个食物订单的方法，传入鱼三明治食物项
        await UsePrepareFoodOrderProcessSingleItemAsync(FoodItem.FishSandwich);
    }

    /// <summary>
    /// 测试单个炸鱼薯条订单的处理流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UseSingleOrderFishAndChipsAsync()
    {
        // 调用处理单个食物订单的方法，传入炸鱼薯条食物项
        await UsePrepareFoodOrderProcessSingleItemAsync(FoodItem.FishAndChips);
    }
    /*
    DISPATCH_SINGLE_ORDER: Dispatching 'Fish & Chips'!
    GATHER_INGREDIENT: Gathered ingredient Potatoes
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Potatoes has been sliced!
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Potatoes got burnt while frying :(
    FRYING_STEP: Ingredient Fish is ready!
    GATHER_INGREDIENT: Gathered ingredient Potatoes
    CUTTING_STEP: Ingredient Potatoes has been sliced!
    FRYING_STEP: Ingredient Potatoes is ready!
    ADD_CONDIMENTS: Added condiments to Fish & Chips - Fish: ["Fish","Fish_gathered","Fish_chopped","Fish_frying_succeeded"], Potatoes: ["Potatoes","Potatoes_gathered","Potatoes_sliced","Potatoes_frying_failed","Potatoes_gathered","Potatoes_sliced","Potatoes_frying_succeeded"]
    PACKING_FOOD: Food Fish Packed! - ["Fish","Fish_gathered","Fish_chopped","Fish_frying_succeeded","Potatoes","Potatoes_gathered","Potatoes_sliced","Potatoes_frying_failed","Potatoes_gathered","Potatoes_sliced","Potatoes_frying_succeeded","Condiments"]    
    */

    /// <summary>
    /// 处理单个食物订单的异步方法
    /// </summary>
    /// <param name="foodItem">要处理的食物项</param>
    /// <returns></returns>
    protected async Task UsePrepareFoodOrderProcessSingleItemAsync(FoodItem foodItem)
    {
        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 构建单个食物项处理流程
        KernelProcess kernelProcess = SingleFoodItemProcess.CreateProcess().Build();

        // 启动流程并传入订单事件
        using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent()
            {
                Id = SingleFoodItemProcess.ProcessEvents.SingleOrderReceived,
                Data = foodItem,
            }
        );
    }
}
