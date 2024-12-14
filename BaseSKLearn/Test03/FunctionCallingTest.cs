using System.Diagnostics.CodeAnalysis;
using BaseSKLearn.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Assistants;

namespace BaseSKLearn;

[Experimental("SKEXP0010")]
public class FunctionCallingTest
{
    public static async Task ManuallyCall_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("Qwen");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        // 注册 kernel function 到 plugins
        kernel.ImportPluginFromFunctions(
            "WeatherPlugin",
            [
                kernel.CreateFunctionFromMethod(
                    GetWeatherForCity,
                    "GetWeatherForCity",
                    "获取指定城市的天气"
                )
            ]
        );

        // 设置OpenAI提示执行的参数，温度设置为0期望更明确的答案，工具调用行为设置为启用内核插件函数，但需要手动调用。
        OpenAIPromptExecutionSettings settings =
            new() { Temperature = 0, ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions };

        // 创建一个聊天历史记录，并添加系统消息和用户消息。
        var chatHistory = new ChatHistory();
        var template = "我想知道现在北京的天气状况？";
        chatHistory.AddSystemMessage("You are a useful assistant.");
        chatHistory.AddUserMessage(template);
        Console.WriteLine($"User: {template}");

        // 获取IChatCompletionService实例，用于处理聊天完成请求。
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        // 开始循环直到没有更多的函数调用或响应为止。
        while (true)
        {
            ChatMessageContent result = await chat.GetChatMessageContentAsync(
                chatHistory,
                settings,
                kernel
            );
            // 如果有响应内容，则输出。
            if (!string.IsNullOrWhiteSpace(result.Content))
                Console.Write("Assistant:" + result.Content);
            // 获取可能的function call列表。
            var functionCalls = FunctionCallContent.GetFunctionCalls(result).ToList();

            // 如果没有函数调用，则退出循环。
            if (functionCalls.Count == 0)
                break;

            // 将包含函数调用（请求）的LLM响应添加到聊天历史记录中，因为LLM需要它。
            chatHistory.Add(result);

            // 遍历所有函数调用，并尝试执行它们。
            foreach (var functionCall in functionCalls)
            {
                try
                {
                    // 调用每个函数并等待结果，将函数的结果添加到聊天历史记录中。
                    FunctionResultContent resultContent = await functionCall.InvokeAsync(kernel); // Executing each function.
                    chatHistory.Add(resultContent.ToChatMessage());
                }
                catch (Exception ex)
                {
                    // 如果函数调用抛出异常，将异常信息添加到聊天历史记录中。
                    chatHistory.Add(new FunctionResultContent(functionCall, ex).ToChatMessage());
                    // 可以选择手动添加自定义错误信息到聊天历史记录中，供LLM推理使用。
                    //string message = "Error details that LLM can reason about.";
                    //chatHistory.Add(new FunctionResultContent(functionCall, message).ToChatMessageContent());
                }
            }
        }
    }

    public static async Task AutoCall_Test()
    {
        var weatherApi = new WeatherAPI(
            ConfigExtensions.FromSecretsConfig<string>("WeatherApiKey")
        );
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("Qwen");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        kernel.ImportPluginFromFunctions(
            "WeatherPlugin",
            [
                kernel.CreateFunctionFromMethod(
                    weatherApi.GetWeatherForCityAsync,
                    "GetWeatherForCityAsync",
                    "获取指定城市的天气"
                )
            ]
        );

        OpenAIPromptExecutionSettings settings =
            new()
            {
                Temperature = 0,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

        var chatHistory = new ChatHistory();
        var template = "我想知道现在北京的天气状况？";
        chatHistory.AddSystemMessage("You are a useful assistant.");
        chatHistory.AddUserMessage(template);
        Console.WriteLine($"User: {template}");
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // 直接请求消息，将自动调用function calling
        var result = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
        Console.Write("Assistant:" + result);
    }

    static string GetWeatherForCity(string cityName)
    {
        return $"{cityName} 25°, 天气晴朗。";
    }
}
