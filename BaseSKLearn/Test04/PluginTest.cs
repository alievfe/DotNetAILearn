using System.Diagnostics.CodeAnalysis;
using BaseSKLearn.Utils;
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
}
