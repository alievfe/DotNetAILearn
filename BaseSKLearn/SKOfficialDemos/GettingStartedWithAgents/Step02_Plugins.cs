using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKUtils;
using SKUtils.SKExtensions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示如何使用 <see cref="KernelPlugin"/> 创建 <see cref="ChatCompletionAgent"/>，
/// 并获取其对明确用户消息的响应。
/// 经过测试，DouBao InternLM 中文下烂，Qwen DeepSeek可以回答
/// </summary>
public class Step02_Plugins
{
    private const string HostName = "主持人";
    private const string HostInstructions = "回答有关菜单的问题。";

    public async Task UseChatCompletionWithPluginAgentAsync()
    {
        // 定义代理
        ChatCompletionAgent agent = new()
        {
            Instructions = HostInstructions,
            Name = HostName,
            Kernel = ConfigExtensions.GetKernel2("Spark"),
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                }
            ),
        };
        // 初始化插件并将其添加到代理的 Kernel 中（与直接使用 Kernel 相同）。
        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        // 创建聊天历史记录以捕获代理的交互。
        ChatHistory chat = [];

        // 响应用户输入，在适当的情况下调用函数。
        await InvokeAgentAsync("你好");
        await InvokeAgentAsync("今天的特色汤是什么？");
        await InvokeAgentAsync("今天的特色饮品是什么？");
        await InvokeAgentAsync("谢谢");

        // 本地函数，用于调用代理并显示对话消息。
        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.Add(message);
            message.WriteAgentChatMessage();

            await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
            {
                chat.Add(response);
                response.WriteAgentChatMessage();
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
}
