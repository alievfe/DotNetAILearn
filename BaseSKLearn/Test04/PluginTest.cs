using System.Diagnostics.CodeAnalysis;
using BaseSKLearn.Plugins.Finefood;
using BaseSKLearn.Plugins.Weather;
using BaseSKLearn.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace BaseSKLearn;

[Experimental("SKEXP0010")]
public class PluginTest
{
    public static async Task S_Translate_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test04", "Plugins");
        string[] pluginNames =  [ "TranslatePlugins" ];
        foreach (var pluginName in pluginNames)
        {
            kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, pluginName));
        }

        // 从插件获得function
        var translaterFunction = kernel.Plugins.GetFunction("TranslatePlugins", "Translator");
        // var translaterFunction = kernel.Plugins["TranslatePlugins"]["Translator"];

        Console.WriteLine("System: 请输入要翻译的内容");
        var userResuest = "期待白昼越过黑夜";
        Console.WriteLine("System: 请输入要翻译的语言语种");
        var language = "英语";

        var result = await translaterFunction.InvokeAsync(
            kernel,
            new() { ["input"] = userResuest, ["language"] = language }
        );

        Console.WriteLine($"Assistant: {result}");
    }

    public static async Task S_Joke_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        // 注册插件，同时直接得到插件列表获取function用于调用
        string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test04", "Plugins");
        var plugin = kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "Writers"));

        // 从插件获得funciton
        var jokeFunction = plugin["Joke"];

        Console.WriteLine("System: 请输入笑话主题");
        var subject = "pig";
        Console.WriteLine("System: 请输入笑话风格");
        var style = "retarded joke";

        var results = await jokeFunction.InvokeAsync(
            kernel,
            new() { ["input"] = subject, ["style"] = style }
        );
        Console.WriteLine($"Assistant: {results}");
    }

    public static async Task N_ImportPluginFromType_Weather_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        // kernel.ImportPluginFromType<WeatherPlugin>();
        // Kernel添加插件
        // var getWeatherFunc = kernel.Plugins.GetFunction(nameof(WeatherPlugin), "WeatherSearch");
        var plugin = kernel.CreatePluginFromType<WeatherPlugin>();
        var getWeatherFunc = plugin[nameof(WeatherPlugin.WeatherSearch)];
        var weatherContent = await kernel.InvokeAsync(getWeatherFunc, new() { ["city"] = "火焰山" });
        Console.WriteLine(weatherContent.ToString());
    }

    public static async Task N_ImportPluginFromObject_Finefood_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<FinefoodPlugin>();
        var rootProvider = services.BuildServiceProvider();
        FinefoodPlugin finefoodPlugin = rootProvider.GetRequiredService<FinefoodPlugin>();
        var plugin = kernel.ImportPluginFromObject(finefoodPlugin);
        var weatherContent = await plugin[nameof(FinefoodPlugin.GetFinefoodList)].InvokeAsync(
            kernel,
            new() { ["city"] = "北京" }
        );
        Console.WriteLine(weatherContent.ToString());
    }

    public static async Task N_ImportPluginFromFunctions_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        // 创建 Kernel Function，以lambda匿名函数创建
        var kf = kernel.CreateFunctionFromMethod(
            (string city) => $"{city} 好玩的地方有八达岭长城，故宫，恭王府等",
            "GetTourismClassic",
            description: "获取城市的经典",
            [ new KernelParameterMetadata(name: "city") { Description = "城市名" } ]
        );
        kernel.ImportPluginFromFunctions("TourismClassicPlugin", [ kf ]);
        var getTourismClassic = kernel
            .Plugins
            .GetFunction("TourismClassicPlugin", "GetTourismClassic");
        var weatherContent = await getTourismClassic.InvokeAsync(kernel, new() { ["city"] = "北京" });
        Console.WriteLine(weatherContent.ToString());
    }
}
