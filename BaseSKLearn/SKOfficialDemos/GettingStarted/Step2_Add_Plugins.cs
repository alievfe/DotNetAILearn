using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKUtils;
using SKUtils.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

/// <summary>
/// 本示例展示了如何加载<see cref="KernelPlugin"/>实例。
/// </summary>
public sealed class Step2_Add_Plugins
// (ITestOutputHelper output) : BaseTest(output)
{
    /// <summary>
    /// 展示了加载<see cref="KernelPlugin"/>实例的不同方法。
    /// </summary>
    public async Task AddPluginsAsync()
    {
        // Create a kernel with OpenAI chat completion
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChat(
            ConfigExtensions
                .LoadConfigFromJson("./tmpsecrets.json")
                .GetSection("DouBao")
                .Get<OpenAIConfig>()
        );

        kernelBuilder.Plugins.AddFromType<TimeInformation>();
        kernelBuilder.Plugins.AddFromType<WidgetFactory>();
        Kernel kernel = kernelBuilder.Build();

        // 示例1。使用提示调用内核，询问AI无法提供的信息，并可能产生幻觉
        Console.WriteLine(await kernel.InvokePromptAsync("现在距离圣诞节已经过去了多少天？"));

        // 示例2。使用模板化提示调用内核，该提示调用插件并显示结果
        Console.WriteLine(
            await kernel.InvokePromptAsync(
                "当前时间是 {{TimeInformation.GetCurrentUtcTime}} 。现在距离圣诞节已经过去了多少天？"
            )
        );

        // 示例3。使用提示调用内核，并允许AI自动调用函数
        OpenAIPromptExecutionSettings settings =
            new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), };
        Console.WriteLine(
            await kernel.InvokePromptAsync("今天距离圣诞节已经过去了多少天？ 解释你的想法。", new(settings))
        );

        // 示例4。使用提示调用内核，并允许AI自动调用使用枚举的函数
        // Useful 随机颜色
        Console.WriteLine(
            await kernel.InvokePromptAsync(
                "Create a handy lime colored widget for me.",
                new(settings)
            )
        );
        // Decorative 随机颜色
        Console.WriteLine(
            await kernel.InvokePromptAsync(
                "Create a beautiful scarlet colored widget for me.",
                new(settings)
            )
        );
        // Decorative 两种颜色
        Console.WriteLine(
            await kernel.InvokePromptAsync(
                "Create an attractive maroon and navy colored widget for me.",
                new(settings)
            )
        );
    }

    /// <summary>
    /// 一个返回当前时间的插件。
    /// </summary>
    private class TimeInformation
    {
        [KernelFunction]
        [Description("获取今天现在的UTC时间")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 一个创建小部件的插件。
    /// </summary>
    private class WidgetFactory
    {
        [KernelFunction]
        [Description("Creates a new widget of the specified type and colors")]
        public WidgetDetails CreateWidget(
            [Description("The type of widget to be created")] WidgetType widgetType,
            [Description("The colors of the widget to be created")] WidgetColor[] widgetColors
        )
        {
            var colors = string.Join('-', widgetColors.Select(c => c.GetDisplayName()).ToArray());
            return new()
            {
                SerialNumber = $"{widgetType}-{colors}-{Guid.NewGuid()}",
                Type = widgetType,
                Colors = widgetColors,
            };
        }
    }

    /// <summary>
    /// <see cref="JsonConverter"/> 用于转换枚举类型
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WidgetType
    {
        [Description("A widget that is useful.")]
        Useful,

        [Description("A widget that is decorative.")]
        Decorative,
    }

    /// <summary>
    /// <see cref="JsonConverter"/> 用于转换枚举类型
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WidgetColor
    {
        [Description("Use when creating a red item.")]
        Red,

        [Description("Use when creating a green item.")]
        Green,

        [Description("Use when creating a blue item.")]
        Blue,
    }

    public class WidgetDetails
    {
        public string SerialNumber { get; init; }
        public WidgetType Type { get; init; }
        public WidgetColor[] Colors { get; init; }
    }
}
