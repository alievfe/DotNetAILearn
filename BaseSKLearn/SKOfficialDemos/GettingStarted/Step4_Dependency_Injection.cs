using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

/// <summary>
/// 此示例展示了如何使用依赖注入（Dependency Injection, DI）与语义内核（Semantic Kernel）。
/// </summary>
public sealed class Step4_Dependency_Injection
{
    /// <summary>
    /// 展示如何创建一个参与依赖注入的 <see cref="Kernel"/> 实例。
    /// </summary>
    public async Task GetKernelUsingDependencyInjectionAsync()
    {
        // 如果应用程序遵循DI准则，下面这行是不必要的，因为DI会将KernelClient类的一个实例注入到引用它的类中。
        // DI容器准则 - https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines#recommendations
        var serviceProvider = BuildServiceProvider();
        var kernel = serviceProvider.GetRequiredService<Kernel>();

        // 使用模板提示调用内核，并将结果流式传输到显示设备
        KernelArguments arguments = new() { { "topic", "从太空看地球" } };
        await foreach (var update in
                       kernel.InvokePromptStreamingAsync("{{$topic}}是什么颜色？请提供详细的解释。", arguments))
        {
            Console.Write(update);
        }
    }

    /// <summary>
    /// 展示如何使用参与依赖注入的插件。
    /// </summary>
    public async Task PluginUsingDependencyInjectionAsync()
    {
        // 如果应用程序遵循DI准则，下面这行是不必要的，因为DI会将KernelClient类的一个实例注入到引用它的类中。
        var serviceProvider = BuildServiceProvider();
        var kernel = serviceProvider.GetRequiredService<Kernel>();

        // 调用依赖于通过依赖注入提供的服务的插件提示。
        PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        Console.WriteLine(await kernel.InvokePromptAsync("按照名字问候当前用户。", new(settings)));
    }

    /// <summary>
    /// 构建一个可以用来解析服务的 ServiceProvider。
    /// </summary>
    private ServiceProvider BuildServiceProvider()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton<IUserService>(new FakeUserService());

        var kernelBuilder = collection.AddKernel();
        var chatConfig = ConfigExtensions.LoadConfigFromJson("./tmpsecrets.json").GetSection("DouBao").Get<OpenAIConfig>();
        kernelBuilder.Services.AddOpenAIChatCompletion(chatConfig.ModelId, chatConfig.ApiKey);
        kernelBuilder.Plugins.AddFromType<TimeInformation>();
        kernelBuilder.Plugins.AddFromType<UserInformation>();

        return collection.BuildServiceProvider();
    }

    /// <summary>
    /// 返回当前时间的插件。
    /// </summary>
    public class TimeInformation(ILoggerFactory loggerFactory)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TimeInformation>();

        [KernelFunction]
        [Description("获取当前UTC时间。")]
        public string GetCurrentUtcTime()
        {
            var utcNow = DateTime.UtcNow.ToString("R");
            this._logger.LogInformation("返回当前时间 {0}", utcNow);
            return utcNow;
        }
    }

    /// <summary>
    /// 返回当前用户名的插件。
    /// </summary>
    public class UserInformation(IUserService userService)
    {
        [KernelFunction]
        [Description("获取当前用户的名称。")]
        public string GetUsername()
        {
            return userService.GetCurrentUsername();
        }
    }

    /// <summary>
    /// 获取当前用户ID的服务接口。
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 返回当前用户的用户名。
        /// </summary>
        string GetCurrentUsername();
    }

    /// <summary>
    /// <see cref="IUserService"/> 的模拟实现。
    /// </summary>
    public class FakeUserService : IUserService
    {
        /// <inheritdoc/>
        public string GetCurrentUsername() => "Bob";
    }
}