using System;
using System.ClientModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils;
using SKUtils.SKExtensions;
using SKUtils.TestUtils;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 此示例演示了使用 <see cref="OpenAIAssistantAgent"/> 和 <see cref="ChatCompletionAgent"/> 的相似性（参见：步骤 2）。
/// </summary>
public class Step08_Assistant(ITestOutputHelper output) : BaseAgentsTest(output)
{
    private const string HostName = "主持人";
    private const string HostInstructions = "回答有关菜单的问题。";

    public async Task UseChatCompletionWithPluginAgentAsync()
    {
        var (provider, modelId) = GetClientProvider();
        // 定义代理
        OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(
            clientProvider: provider,
            definition: new OpenAIAssistantDefinition(modelId)
            {
                Instructions = HostInstructions,
                Name = HostName,
                Metadata = AssistantSampleMetadata,
            },
            kernel: new Kernel()
        );
        // 初始化插件并将其添加到代理的 Kernel 中（与直接使用 Kernel 相同）。
        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        string threadId = await agent.CreateThreadAsync(
            new OpenAIThreadCreationOptions() { Metadata = AssistantSampleMetadata }
        );

        // 响应用户输入
        try
        {
            await InvokeAgentAsync("你好");
            await InvokeAgentAsync("今天的特色汤是什么？价格是多少？");
            await InvokeAgentAsync("今天的特色饮品是什么？价格是多少？");
            await InvokeAgentAsync("谢谢");
        }
        finally
        {
            // 清理资源：删除线程和代理。
            await agent.DeleteThreadAsync(threadId);
            await agent.DeleteAsync();
        }

        // 本地函数，用于调用代理并显示对话消息。
        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            await agent.AddChatMessageAsync(threadId, message);
            message.WriteAgentChatMessage();

            await foreach (ChatMessageContent response in agent.InvokeAsync(threadId))
            {
                response.WriteAgentChatMessage();
            }
        }
    }

     [Fact]
    public async Task UseTemplateForAssistantAgentAsync()
    {
        // 定义代理
        var promptYaml = await File.ReadAllTextAsync("./Resources/GenerateStory.yaml");        
        PromptTemplateConfig templateConfig = KernelFunctionYaml.ToPromptTemplateConfig(promptYaml);

        // 通过配置文件定义指令、名称和描述属性。
        var (provider, modelId) = GetClientProvider();
        OpenAIAssistantAgent agent =
            await OpenAIAssistantAgent.CreateFromTemplateAsync(
                clientProvider: provider,
                capabilities: new OpenAIAssistantCapabilities(modelId)
                {
                    Metadata = AssistantSampleMetadata,
                },
                kernel: new Kernel(),
                defaultArguments: new KernelArguments()
                {
                    { "topic", "Dog" },
                    { "length", "3" },
                },
                templateConfig);

        // 为代理对话创建一个线程。
        string threadId = await agent.CreateThreadAsync(new OpenAIThreadCreationOptions { Metadata = AssistantSampleMetadata });

        try
        {
            // 使用默认参数调用代理。
            await InvokeAgentAsync();

            // 使用覆盖参数调用代理。
            await InvokeAgentAsync(
                new()
                {
                { "topic", "Cat" },
                { "length", "3" },
                });
        }
        finally
        {
            // 清理资源：删除线程和代理。
            await agent.DeleteThreadAsync(threadId);
            await agent.DeleteAsync();
        }

        // 本地函数，用于调用代理并显示响应。
        async Task InvokeAgentAsync(KernelArguments? arguments = null)
        {
            await foreach (ChatMessageContent response in agent.InvokeAsync(threadId, arguments))
            {
                WriteAgentChatMessage(response);
            }
        }
    }

    private sealed class MenuPlugin
    {
        [KernelFunction, Description("提供菜单中的特色菜品列表。")]
        public string GetSpecials() =>
            """
                特色汤品：西湖牛肉羹
                特色沙拉：凉拌黄瓜
                特色饮品：茉莉花茶
                """;

        [KernelFunction, Description("提供所请求菜单项的价格。")]
        public string GetItemPrice([Description("菜单项的名称。")] string menuItem) => "¥28.00";
    }

    protected static readonly ReadOnlyDictionary<string, string> AssistantSampleMetadata = new(
        new Dictionary<string, string> { { "sksample", bool.TrueString } }
    );
}
