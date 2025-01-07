using System;
using HandlebarsDotNet.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

/// <summary>
/// 本示例展示了如何加载一个 Open API <see cref="KernelPlugin"/> 实例。
/// </summary>
public class Step9_OpenAPI_Plugins
{
    public async Task AddOpenAPIPluginsAsync()
    {
        var kernel = ConfigExtensions.GetKernel("DouBao");

        // 加载 OpenAPI 插件
        var plugin = await kernel.ImportPluginFromOpenApiAsync(
            "RepairService",
            "./Resources/repair-service.json"
        );
        PromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };
        Console.WriteLine(
            await kernel.InvokePromptAsync("List all of the repairs .", new(settings))
        );
    }

    /// <summary>
    /// 展示了如何转换一个 Open API <see cref="KernelPlugin"/> 实例以支持依赖注入。
    /// </summary>
    public async Task TransformOpenAPIPluginsAsync()
    {
        // 创建一个带有 OpenAI 聊天完成功能的内核
        var serviceProvider = BuildServiceProvider();
        var kernel = serviceProvider.GetRequiredService<Kernel>();

        // 加载 OpenAPI 插件
        var plugin = await kernel.CreatePluginFromOpenApiAsync(
            "RepairService",
            "./Resources/repair-service.json"
        );

        // 转换插件以使用 IMechanicService 通过依赖注入
        kernel.Plugins.Add(TransformPlugin(plugin));

        PromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };

        // 预约一次更换旧发动机油并替换为新机油的服务.
        Console.WriteLine(
            await kernel.InvokePromptAsync(
                "Book an appointment to drain the old engine oil and replace it with fresh oil.",
                new(settings)
            )
        );
    }

    /// <summary>
    /// 构建一个可以用来解析服务的 ServiceProvider。
    /// </summary>
    private ServiceProvider BuildServiceProvider()
    {
        ServiceCollection collection = [];
        collection.AddSingleton<IMechanicService>(new FakeMechanicService());
        var chatConfig = ConfigExtensions.GetConfig<OpenAIConfig>("./tmpsecrets.json", "DouBao");
        collection
            .AddKernel()
            .AddOpenAIChatCompletion(
                modelId: chatConfig.ModelId,
                apiKey: chatConfig.ApiKey,
                endpoint: chatConfig.Endpoint
            );
        return collection.BuildServiceProvider();
    }

    /// <summary>
    /// 修改 KernelPlugin 实例中特定函数的行为，而不改变其他函数。此处改变 createRepair 函数的行为。
    /// </summary>
    public static KernelPlugin TransformPlugin(KernelPlugin plugin)
    {
        List<KernelFunction>? functions = [];
        foreach (var function in plugin)
        {
            if (function.Name == "createRepair")
            {
                functions.Add(CreateRepairFunction(function));
            }
            else
            {
                functions.Add(function);
            }
        }
        return KernelPluginFactory.CreateFromFunctions(plugin.Name, plugin.Description, functions);
    }

    /// <summary>
    /// 创建一个用于 createRepair 操作的 <see cref="KernelFunction"/> 实例，该实例只接收
    /// title 和 description 参数，并且有一个委托使用 IMechanicService 来获取 assignedTo。
    /// 相当于和原先引入openapi中的createRepair减少了几个参数 
    /// </summary>
    public static KernelFunction CreateRepairFunction(KernelFunction function) =>
        KernelFunctionFactory.CreateFromMethod(
            (
                Kernel kernel,
                KernelFunction currentFunction,
                KernelArguments arguments,
                [FromKernelServices] IMechanicService mechanicService,
                CancellationToken cancellationToken
            ) =>
            {
                arguments.Add("assignedTo", mechanicService.GetMechanic());
                arguments.Add("date", DateTime.UtcNow.ToString("R"));

                return function.InvokeAsync(kernel, arguments, cancellationToken);
            },
            new KernelFunctionFromMethodOptions()
            {
                FunctionName = function.Name,
                Description = function.Description,
                Parameters = function
                    .Metadata.Parameters.Where(e => e.Name == "title" || e.Name == "description")
                    .ToList(),
                ReturnParameter = function.Metadata.ReturnParameter,
            }
        );

    /// <summary>
    /// 获取分配给下一个工作的技工的服务接口。
    /// </summary>
    public interface IMechanicService
    {
        /// <summary>
        /// 返回分配给下一个工作的技工的名字。
        /// </summary>
        string GetMechanic();
    }

    /// <summary>
    /// <see cref="IMechanicService"/> 的模拟实现。
    /// </summary>
    public class FakeMechanicService : IMechanicService
    {
        public string GetMechanic() => "Bob";
    }
}
