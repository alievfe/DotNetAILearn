using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

namespace BaseSKLearn;

[Experimental("SKEXP0010")]
public class HttpPluginTest(Kernel kernel)
{
    public async Task Test()
    {
        // 导入HttpPlugin
        var httpPlugin = kernel.ImportPluginFromType<HttpPlugin>();

        //var httpclient = new HttpClient();
        //kernel.ImportPluginFromObject(new HttpPlugin(httpclient));


        var history = new ChatHistory();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        Console.Write("User > ");
        while (Console.ReadLine() is { } userInput)
        {
            history.AddUserMessage(userInput);

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            };
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel
            );

            Console.WriteLine("Assistant > " + result);
            history.AddMessage(result.Role, result.Content ?? string.Empty);

            Console.Write("User > ");
        }
        Console.ReadKey();
    }
}
