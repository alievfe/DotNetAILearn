using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils;
using SKUtils.SKExtensions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示通过依赖注入创建代理，使用注册的服务。
/// </summary>
public class Step06_DependencyInjection
{
    // 导师
    private const string TutorName = "Tutor";

    /*
        逐步思考，并从创造力和表达力方面对用户输入进行评分，评分范围为1-100。

        以JSON格式响应，遵循以下JSON模式：

        {
            "score": "整数 (1-100)",
            "notes": "评分的原因"
        }
    */
    private const string TutorInstructions = """
        Think step-by-step and rate the user input on creativity and expressiveness from 1-100.

        Respond in JSON format with the following JSON schema:

        {
            "score": "integer (1-100)",
            "notes": "the reason for your score"
        }
        """;

    public async Task UseDependencyInjectionToCreateAgentAsync()
    {
        ServiceCollection serviceContainer = new();
        serviceContainer.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));
        var chatConfig = ConfigExtensions.GetConfig<OpenAIConfig>("./tmpsecrets.json", "DouBao");
        serviceContainer.AddOpenAIChatCompletion(
            modelId: chatConfig.ModelId,
            apiKey: chatConfig.ApiKey,
            endpoint: chatConfig.Endpoint
        );
        // 瞬态Kernel，因为每个代理可能会通过插件自定义其Kernel实例。
        serviceContainer.AddTransient<Kernel>();

        serviceContainer.AddTransient<AgentClient>();

        serviceContainer.AddKeyedSingleton<ChatCompletionAgent>(
            TutorName,
            (sp, key) =>
                new ChatCompletionAgent()
                {
                    Instructions = TutorInstructions,
                    Name = TutorName,
                    Kernel = sp.GetRequiredService<Kernel>().Clone(),
                }
        );

        // 创建一个服务提供者以解析已注册的服务
        await using ServiceProvider serviceProvider = serviceContainer.BuildServiceProvider();

        // 如果应用程序遵循DI指南，以下代码行是不必要的，因为DI会将AgentClient类的实例注入到引用它的类中。
        AgentClient agentClient = serviceProvider.GetRequiredService<AgentClient>();

        // 执行代理客户端
        await WriteAgentResponse("The sunset is nice.");
        await WriteAgentResponse("The sunset is setting over the mountains.");
        await WriteAgentResponse(
            "The sunset is setting over the mountains and filled the sky with a deep red flame, setting the clouds ablaze."
        );

        // 本地函数，用于调用代理并显示聊天消息。
        async Task WriteAgentResponse(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            message.WriteAgentChatMessage();

            await foreach (ChatMessageContent response in agentClient.RunDemoAsync(message))
            {
                response.WriteAgentChatMessage();
            }
        }
    }

    private sealed class AgentClient([FromKeyedServices(TutorName)] ChatCompletionAgent agent)
    {
        private readonly AgentGroupChat _chat = new();

        public IAsyncEnumerable<ChatMessageContent> RunDemoAsync(ChatMessageContent input)
        {
            this._chat.AddChatMessage(input);

            return this._chat.InvokeAsync(agent);
        }
    }

    private record struct WritingScore(int score, string notes);
}
