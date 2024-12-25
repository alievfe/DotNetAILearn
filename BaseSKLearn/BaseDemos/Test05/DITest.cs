using System.Diagnostics.CodeAnalysis;
using BaseSKLearn.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKUtils;

namespace BaseSKLearn;

[Experimental("SKEXP0010")]
public class DITest
{
    public static async Task Test()
    {
        await Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder => { })
            .ConfigureServices(
                (hostContext, services) =>
                {
                    var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig, Program>("DouBao");

                    // 注册kernel
                    services
                        .AddKernel()
                        .AddOpenAIChatCompletion(
                            modelId: config.ModelId,
                            apiKey: config.ApiKey,
                            endpoint: config.Endpoint
                        );

                    // 注册插件，将自动导入进kernel中
                    services.AddSingleton(
                        sp => KernelPluginFactory.CreateFromType<LightPlugin>(serviceProvider: sp)
                    );

                    services.AddHostedService<Worker>();
                }
            )
            .RunConsoleAsync();
    }
}

public class Worker(IHostApplicationLifetime hostLifetime, Kernel kernel) : IHostedService
{
    private int? _exitCode;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Create chat history
            var history = new ChatHistory();

            // Get chat completion service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            // Start the conversation
            Console.Write("User > ");
            string? userInput;
            while ((userInput = Console.ReadLine()) is not null)
            {
                // Add user input
                history.AddUserMessage(userInput);

                // Enable auto function calling
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings =
                    new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

                // Get the response from the AI
                // 此处将自动调用plugin的function calling
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel
                );

                // Print the results
                Console.WriteLine("Assistant > " + result);

                // Add the message from the agent to the chat history
                history.AddMessage(result.Role, result.Content ?? string.Empty);

                // Get user input again
                Console.Write("User > ");
            }

            _exitCode = 0;
        }
        catch (Exception)
        {
            _exitCode = 1;
        }
        finally
        {
            hostLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
        return Task.CompletedTask;
    }
}
