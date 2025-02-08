using System;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03.Processes;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process.Models;
using Microsoft.SemanticKernel.Process.Tools;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step03;

/// <summary>
/// 展示步骤的重用、流程的创建、多个事件的生成、对食品制备样品使用有状态步骤。
/// 演示如何创建 <see cref="KernelProcess"/> 并引发不同的食物相关事件。
/// 有关此处使用的流程的视觉参考，请查看以下链接中的图表：https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/README.md#step03a_foodPreparation
/// </summary>
public class Step03a_FoodPreparation
{
    // Step03a 用于从/向存储库保存和加载 SK 流程的工具
    private readonly string _step03RelativePath = Path.Combine(
        "SKOfficialDemos",
        "GettingStartedWithProcesses",
        "Step03",
        "ProcessesStates"
    );
    private readonly string _statefulFriedFishProcessFilename = "FriedFishProcessStateSuccess.json";
    private readonly string _statefulFishSandwichProcessFilename =
        "FishSandwichStateProcessSuccess.json";
    private readonly string _statefulFriedFishLowStockProcessFilename =
        "FriedFishProcessStateSuccessLowStock.json";
    private readonly string _statefulFriedFishNoStockProcessFilename =
        "FriedFishProcessStateSuccessNoStock.json";
    private readonly string _statefulFishSandwichLowStockProcessFilename =
        "FishSandwichStateProcessSuccessLowStock.json";

    #region 无状态流程
    [Fact]
    public async Task UsePrepareFriedFishProcessAsync()
    {
        var process = FriedFishProcess.CreateProcess();
        await UsePrepareSpecificProductAsync(
            process,
            FriedFishProcess.ProcessEvents.PrepareFriedFish
        );
    }

    /*
    === Start SK Process 'FriedFishProcess' ===
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish is ready!
    === End SK Process 'FriedFishProcess' ===
    */

    [Fact]
    public async Task UsePreparePotatoFriesProcessAsync()
    {
        var process = PotatoFriesProcess.CreateProcess();
        await UsePrepareSpecificProductAsync(
            process,
            PotatoFriesProcess.ProcessEvents.PreparePotatoFries
        );
    }

    /*
    === Start SK Process 'PotatoFriesProcess' ===
    GATHER_INGREDIENT: Gathered ingredient Potatoes
    CUTTING_STEP: Ingredient Potatoes has been sliced!
    FRYING_STEP: Ingredient Potatoes is ready!
    === End SK Process 'PotatoFriesProcess' ===
    */

    [Fact]
    public async Task UsePrepareFishSandwichProcessAsync()
    {
        var process = FishSandwichProcess.CreateProcess();

        string mermaidGraph = process.ToMermaid(1);
        Console.WriteLine($"=== Start - Mermaid Diagram for '{process.Name}' ===");
        Console.WriteLine(mermaidGraph);
        Console.WriteLine($"=== End - Mermaid Diagram for '{process.Name}' ===");

        await UsePrepareSpecificProductAsync(
            process,
            FishSandwichProcess.ProcessEvents.PrepareFishSandwich
        );
    }

    /*
    === Start - Mermaid Diagram for 'FishSandwichProcess' ===
    flowchart LR
        Start["Start"]
        End["End"]
        FriedFishProcess[["FriedFishProcess"]]
        FriedFishProcess["FriedFishProcess"] --> AddBunsStep["AddBunsStep"]
        AddBunsStep["AddBunsStep"]
        AddBunsStep["AddBunsStep"] --> AddSpecialSauceStep["AddSpecialSauceStep"]
        AddSpecialSauceStep["AddSpecialSauceStep"]
        AddSpecialSauceStep["AddSpecialSauceStep"] --> ExternalFriedFishStep["ExternalFriedFishStep"]
        ExternalFriedFishStep["ExternalFriedFishStep"]
        Start --> FriedFishProcess["FriedFishProcess"]
        ExternalFriedFishStep["ExternalFriedFishStep"] --> End

    === End - Mermaid Diagram for 'FishSandwichProcess' ===");
    === Start SK Process 'FishSandwichProcess' ===
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish is ready!
    BUNS_ADDED_STEP: Buns added to ingredient Fish
    SPECIAL_SAUCE_ADDED: Special sauce added to ingredient Fish
    === End SK Process 'FishSandwichProcess' ===
    */

    [Fact]
    public async Task UsePrepareFishAndChipsProcessAsync()
    {
        var process = FishAndChipsProcess.CreateProcess();
        await UsePrepareSpecificProductAsync(
            process,
            FishAndChipsProcess.ProcessEvents.PrepareFishAndChips
        );
    }
    /*
    === Start SK Process 'FishAndChipsProcess' ===
    GATHER_INGREDIENT: Gathered ingredient Potatoes
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Potatoes has been sliced!
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Potatoes got burnt while frying :(
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Potatoes
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Potatoes has been sliced!
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Potatoes got burnt while frying :(
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Potatoes
    GATHER_INGREDIENT: Gathered ingredient Fish
    CUTTING_STEP: Ingredient Potatoes has been sliced!
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Potatoes is ready!
    FRYING_STEP: Ingredient Fish is ready!
    ADD_CONDIMENTS: Added condiments to Fish & Chips - Fish: ["Fish","Fish_gathered","Fish_chopped","Fish_frying_failed","Fish_gathered","Fish_chopped","Fish_frying_failed","Fish_gathered","Fish_chopped","Fish_frying_succeeded"], Potatoes: ["Potatoes","Potatoes_gathered","Potatoes_sliced","Potatoes_frying_failed","Potatoes_gathered","Potatoes_sliced","Potatoes_frying_failed","Potatoes_gathered","Potatoes_sliced","Potatoes_frying_succeeded"]
    === End SK Process 'FishAndChipsProcess' ===
    */
    #endregion


    #region 有状态流程
    /// <summary>
    /// 该测试用例展示了当同一个流程被多次构建时，每次构建的流程将具有不同的初始状态。
    /// 此时采集原料步骤存储了食材储量，每个订单如果食材被炸烂了，会重新拿出一个食材切了再炸。第二个订单还是有5个食材储量，不会受到第一个订单的影响。
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UsePrepareStatefulFriedFishProcessNoSharedStateAsync()
    {
        // 创建带有状态步骤的炸鱼流程构建器
        var processBuilder = FriedFishProcess.CreateProcessWithStatefulStepsV1();
        // 获取外部触发事件名称
        var externalTriggerEvent = FriedFishProcess.ProcessEvents.PrepareFriedFish;

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");

        // 断言
        Console.WriteLine($"=== 启动 SK 流程 '{processBuilder.Name}' ===");
        // 执行第一次流程
        await ExecuteProcessWithStateAsync(
            processBuilder.Build(),
            kernel,
            externalTriggerEvent,
            "订单 1"
        );
        // 再次构建流程并执行
        await ExecuteProcessWithStateAsync(
            processBuilder.Build(),
            kernel,
            externalTriggerEvent,
            "订单 2"
        );
        Console.WriteLine($"=== 结束 SK 流程 '{processBuilder.Name}' ===");
    }

    /*
    === 启动 SK 流程 'FriedFishWithStatefulStepsProcess' ===
    === 订单 1 ===
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 4
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 3
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 2
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 1
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish is ready!
    === 订单 2 ===
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 4
    CUTTING_STEP: Ingredient Fish has been chopped!
    FRYING_STEP: Ingredient Fish is ready!
    === 结束 SK 流程 'FriedFishWithStatefulStepsProcess' ===
    */

    /// <summary>
    /// 该测试用例展示了当同一个流程只构建一次并多次使用时，流程将共享状态，
    /// 并且步骤的状态将成为下一次运行流程的初始状态。
    /// 每次订单影响的食材数量、刀具锋利度等都会一直累积。
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UsePrepareStatefulFriedFishProcessSharedStateAsync()
    {
        // 创建带有状态步骤的炸鱼流程构建器
        var processBuilder = FriedFishProcess.CreateProcessWithStatefulStepsV2();
        // 获取外部触发事件名称
        var externalTriggerEvent = FriedFishProcess.ProcessEvents.PrepareFriedFish;

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 只构建一次内核流程，三次执行流程状态会被保留
        KernelProcess kernelProcess = processBuilder.Build();

        Console.WriteLine($"=== 启动 SK 流程 '{processBuilder.Name}' ===");
        // 执行第一次流程
        await ExecuteProcessWithStateAsync(kernelProcess, kernel, externalTriggerEvent, "订单 1");
        // 再次执行流程
        await ExecuteProcessWithStateAsync(kernelProcess, kernel, externalTriggerEvent, "订单 2");
        // 第三次执行流程
        await ExecuteProcessWithStateAsync(kernelProcess, kernel, externalTriggerEvent, "订单 3");
        Console.WriteLine($"=== 结束 SK 流程 '{processBuilder.Name}' ===");
    }

    /*
    === 启动 SK 流程 'FriedFishWithStatefulStepsProcess' ===
    === 订单 1 ===
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 4
    CUTTING_STEP: Ingredient Fish has been chopped! - knife sharpness: 4
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 3
    CUTTING_STEP: Ingredient Fish has been chopped! - knife sharpness: 3
    FRYING_STEP: Ingredient Fish is ready!
    === 订单 2 ===
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 2
    CUTTING_STEP: Dull knife, cannot chop Fish - needs sharpening.
    KNIFE SHARPENED: Knife sharpness is now 8!
    CUTTING_STEP: Ingredient Fish has been chopped! - knife sharpness: 7
    FRYING_STEP: Ingredient Fish got burnt while frying :(
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 1
    CUTTING_STEP: Ingredient Fish has been chopped! - knife sharpness: 6
    FRYING_STEP: Ingredient Fish is ready!
    === 订单 3 ===
    GATHER_INGREDIENT: Gathered ingredient Fish - remaining: 0
    CUTTING_STEP: Ingredient Fish has been chopped! - knife sharpness: 5
    FRYING_STEP: Ingredient Fish is ready!
    === 结束 SK 流程 'FriedFishWithStatefulStepsProcess' ===
    */

    /// <summary>
    /// 该测试用例展示了如何使用有状态步骤来制作炸薯条。
    /// 该流程将共享状态，并且步骤的状态将成为下一次运行流程的初始状态。
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UsePrepareStatefulPotatoFriesProcessSharedStateAsync()
    {
        // 创建带有状态步骤的薯条流程构建器
        var processBuilder = PotatoFriesProcess.CreateProcessWithStatefulSteps();
        // 获取外部触发事件名称
        var externalTriggerEvent = PotatoFriesProcess.ProcessEvents.PreparePotatoFries;

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 构建内核流程
        KernelProcess kernelProcess = processBuilder.Build();

        Console.WriteLine($"=== 启动 SK 流程 '{processBuilder.Name}' ===");
        // 执行第一次流程
        await ExecuteProcessWithStateAsync(kernelProcess, kernel, externalTriggerEvent, "订单 1");
        // 再次执行流程
        await ExecuteProcessWithStateAsync(kernelProcess, kernel, externalTriggerEvent, "订单 2");
        // 第三次执行流程
        await ExecuteProcessWithStateAsync(kernelProcess, kernel, externalTriggerEvent, "订单 3");
        Console.WriteLine($"=== 结束 SK 流程 '{processBuilder.Name}' ===");
    }

    /// <summary>
    /// 执行带有状态的流程
    /// </summary>
    /// <param name="process">要执行的内核流程</param>
    /// <param name="kernel">内核实例</param>
    /// <param name="externalTriggerEvent">外部触发事件名称</param>
    /// <param name="orderLabel">订单标签，默认为 "订单 1"</param>
    /// <returns>返回执行后的内核流程状态</returns>
    private async Task<KernelProcess> ExecuteProcessWithStateAsync(
        KernelProcess process,
        Kernel kernel,
        string externalTriggerEvent,
        string orderLabel = "订单 1"
    )
    {
        Console.WriteLine($"=== {orderLabel} ===");
        // 启动流程
        var runningProcess = await process.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = externalTriggerEvent, Data = new List<string>() }
        );
        // 获取流程状态
        return await runningProcess.GetStateAsync();
    }

    #region 运行流程并将流程状态元数据保存到本地文件
    /// <summary>
    /// 运行并存储有状态的炸鱼流程的状态
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunAndStoreStatefulFriedFishProcessStateAsync()
    {
        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建有状态的炸鱼流程构建器
        ProcessBuilder builder = FriedFishProcess.CreateProcessWithStatefulStepsV1();
        // 构建炸鱼流程
        KernelProcess friedFishProcess = builder.Build();

        // 执行炸鱼流程
        var executedProcess = await ExecuteProcessWithStateAsync(
            friedFishProcess,
            kernel,
            externalTriggerEvent: FriedFishProcess.ProcessEvents.PrepareFriedFish
        );
        // 获取流程的状态元数据
        var processState = executedProcess.ToProcessStateMetadata();
        // 将流程状态元数据保存到本地文件
        DumpProcessStateMetadataLocally(processState, _statefulFriedFishProcessFilename);
    }

    /// <summary>
    /// 运行并存储有状态的鱼三明治流程的状态
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunAndStoreStatefulFishSandwichProcessStateAsync()
    {
        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建有状态的鱼三明治流程构建器
        ProcessBuilder builder = FishSandwichProcess.CreateProcessWithStatefulStepsV1();
        // 构建鱼三明治流程
        KernelProcess friedFishProcess = builder.Build();

        // 执行鱼三明治流程
        var executedProcess = await ExecuteProcessWithStateAsync(
            friedFishProcess,
            kernel,
            externalTriggerEvent: FishSandwichProcess.ProcessEvents.PrepareFishSandwich
        );
        // 获取流程的状态元数据
        var processState = executedProcess.ToProcessStateMetadata();
        // 将流程状态元数据保存到本地文件
        DumpProcessStateMetadataLocally(processState, _statefulFishSandwichProcessFilename);
    }

    private void DumpProcessStateMetadataLocally(
        KernelProcessStateMetadata processStateInfo,
        string jsonFilename
    )
    {
        var sampleRelativePath = GetSampleStep03Filepath(jsonFilename);
        ProcessStateMetadataUtilities.DumpProcessStateMetadataLocally(
            processStateInfo,
            sampleRelativePath
        );
    }

    private string GetSampleStep03Filepath(string jsonFilename)
    {
        return Path.Combine(this._step03RelativePath, jsonFilename);
    }
    #endregion

    #region 从本地文件读取状态并应用到现有的流程构建器
    /// <summary>
    /// 从文件中读取有状态的炸鱼流程状态并运行该流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunStatefulFriedFishProcessFromFileAsync()
    {
        // 从文件中加载有状态的炸鱼流程的状态元数据
        var processState = LoadProcessStateMetadata(this._statefulFriedFishProcessFilename);
        // 断言加载的状态元数据不为空
        Assert.NotNull(processState);

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建有状态的炸鱼流程构建器
        ProcessBuilder processBuilder = FriedFishProcess.CreateProcessWithStatefulStepsV1();
        // 使用加载的状态元数据构建流程
        KernelProcess processFromFile = processBuilder.Build(processState);

        // 执行流程
        await ExecuteProcessWithStateAsync(
            processFromFile,
            kernel,
            externalTriggerEvent: FriedFishProcess.ProcessEvents.PrepareFriedFish
        );
    }

    /// <summary>
    /// 从文件中读取低库存的有状态炸鱼流程状态并运行该流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunStatefulFriedFishProcessWithLowStockFromFileAsync()
    {
        // 从文件中加载低库存的有状态炸鱼流程的状态元数据
        var processState = LoadProcessStateMetadata(this._statefulFriedFishLowStockProcessFilename);
        // 断言加载的状态元数据不为空
        Assert.NotNull(processState);

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建有状态的炸鱼流程构建器
        ProcessBuilder processBuilder = FriedFishProcess.CreateProcessWithStatefulStepsV1();
        // 使用加载的状态元数据构建流程
        KernelProcess processFromFile = processBuilder.Build(processState);

        // 执行流程
        await ExecuteProcessWithStateAsync(
            processFromFile,
            kernel,
            externalTriggerEvent: FriedFishProcess.ProcessEvents.PrepareFriedFish
        );
    }

    /// <summary>
    /// 从文件中读取无库存的有状态炸鱼流程状态并运行该流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunStatefulFriedFishProcessWithNoStockFromFileAsync()
    {
        // 从文件中加载无库存的有状态炸鱼流程的状态元数据
        var processState = LoadProcessStateMetadata(this._statefulFriedFishNoStockProcessFilename);
        // 断言加载的状态元数据不为空
        Assert.NotNull(processState);

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建有状态的炸鱼流程构建器
        ProcessBuilder processBuilder = FriedFishProcess.CreateProcessWithStatefulStepsV1();
        // 使用加载的状态元数据构建流程
        KernelProcess processFromFile = processBuilder.Build(processState);

        // 执行流程
        await ExecuteProcessWithStateAsync(
            processFromFile,
            kernel,
            externalTriggerEvent: FriedFishProcess.ProcessEvents.PrepareFriedFish
        );
    }

    /// <summary>
    /// 从文件中读取有状态的鱼三明治流程状态并运行该流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunStatefulFishSandwichProcessFromFileAsync()
    {
        // 从文件中加载有状态的鱼三明治流程的状态元数据
        var processState = LoadProcessStateMetadata(this._statefulFishSandwichProcessFilename);
        // 断言加载的状态元数据不为空
        Assert.NotNull(processState);

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建有状态的鱼三明治流程构建器
        ProcessBuilder processBuilder = FishSandwichProcess.CreateProcessWithStatefulStepsV1();
        // 使用加载的状态元数据构建流程
        KernelProcess processFromFile = processBuilder.Build(processState);

        // 执行流程
        await ExecuteProcessWithStateAsync(
            processFromFile,
            kernel,
            externalTriggerEvent: FishSandwichProcess.ProcessEvents.PrepareFishSandwich
        );
    }

    /// <summary>
    /// 从文件中读取低库存的有状态鱼三明治流程状态并运行该流程
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunStatefulFishSandwichProcessWithLowStockFromFileAsync()
    {
        // 从文件中加载低库存的有状态鱼三明治流程的状态元数据
        var processState = LoadProcessStateMetadata(
            this._statefulFishSandwichLowStockProcessFilename
        );
        // 断言加载的状态元数据不为空
        Assert.NotNull(processState);

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建有状态的鱼三明治流程构建器
        ProcessBuilder processBuilder = FishSandwichProcess.CreateProcessWithStatefulStepsV1();
        // 使用加载的状态元数据构建流程
        KernelProcess processFromFile = processBuilder.Build(processState);

        // 执行流程
        await ExecuteProcessWithStateAsync(
            processFromFile,
            kernel,
            externalTriggerEvent: FishSandwichProcess.ProcessEvents.PrepareFishSandwich
        );
    }
    #endregion

    #region 版本兼容性场景：加载旧版本流程生成的状态
    /// <summary>
    /// 从文件中读取旧版本低库存的有状态炸鱼流程状态，并应用到新版本的流程构建器上运行
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunStatefulFriedFishV2ProcessWithLowStockV1StateFromFileAsync()
    {
        // 从文件中加载旧版本低库存的有状态炸鱼流程的状态元数据
        var processState = LoadProcessStateMetadata(this._statefulFriedFishLowStockProcessFilename);
        // 断言加载的状态元数据不为空
        Assert.NotNull(processState);

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建新版本有状态的炸鱼流程构建器
        ProcessBuilder processBuilder = FriedFishProcess.CreateProcessWithStatefulStepsV2();
        // 使用加载的状态元数据构建流程
        KernelProcess processFromFile = processBuilder.Build(processState);

        // 执行流程
        await ExecuteProcessWithStateAsync(
            processFromFile,
            kernel,
            externalTriggerEvent: FriedFishProcess.ProcessEvents.PrepareFriedFish
        );
    }

    /// <summary>
    /// 从文件中读取旧版本低库存的有状态鱼三明治流程状态，并应用到新版本的流程构建器上运行
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunStatefulFishSandwichV2ProcessWithLowStockV1StateFromFileAsync()
    {
        // 从文件中加载旧版本低库存的有状态鱼三明治流程的状态元数据
        var processState = LoadProcessStateMetadata(
            this._statefulFishSandwichLowStockProcessFilename
        );
        // 断言加载的状态元数据不为空
        Assert.NotNull(processState);

        // 创建带有聊天完成功能的内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");
        // 创建新版本有状态的鱼三明治流程构建器
        ProcessBuilder processBuilder = FishSandwichProcess.CreateProcessWithStatefulStepsV2();
        // 使用加载的状态元数据构建流程
        KernelProcess processFromFile = processBuilder.Build(processState);

        // 执行流程
        await ExecuteProcessWithStateAsync(
            processFromFile,
            kernel,
            externalTriggerEvent: FishSandwichProcess.ProcessEvents.PrepareFishSandwich
        );
    }
    #endregion


    #endregion
    private KernelProcessStateMetadata? LoadProcessStateMetadata(string jsonFilename)
    {
        var sampleRelativePath = GetSampleStep03Filepath(jsonFilename);
        return ProcessStateMetadataUtilities.LoadProcessStateMetadata(sampleRelativePath);
    }

    protected async Task UsePrepareSpecificProductAsync(
        ProcessBuilder processBuilder,
        string externalTriggerEvent
    )
    {
        // Arrange
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");

        // Act
        KernelProcess kernelProcess = processBuilder.Build();

        // Assert
        Console.WriteLine($"=== Start SK Process '{processBuilder.Name}' ===");
        using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent() { Id = externalTriggerEvent, Data = new List<string>() }
        );
        Console.WriteLine($"=== End SK Process '{processBuilder.Name}' ===");
    }
}
