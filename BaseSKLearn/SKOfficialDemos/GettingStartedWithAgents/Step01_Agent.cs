using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils;
using SKUtils.SKExtensions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示如何创建 <see cref="ChatCompletionAgent"/> 并
/// 获取其对三条明确用户消息的响应。
/// </summary>
public class Step01_Agent
{
    private const string ParrotName = "鹦鹉";
    private const string ParrotInstructions = "以海盗的口吻重复用户的消息，然后以鹦鹉的声音结束。";

    /// <summary>
    /// 使用单个
    /// </summary>
    public async Task UseSingleChatComplectionAgentAsync()
    {
        ChatCompletionAgent agent = new()
        {
            Name = ParrotName,
            Instructions = ParrotInstructions,
            Kernel = ConfigExtensions.GetKernel2("DouBao"),
        };
        ChatHistory chat = [];
        // 响应用户输入
        await InvokeAgentAsync("命运眷顾勇者。");
        await InvokeAgentAsync("我来，我见，我征服。");
        await InvokeAgentAsync("熟能生巧。");

        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent msg = new(AuthorRole.User, input);
            chat.Add(msg);
            msg.WriteAgentChatMessage();
            await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
            {
                chat.Add(response);
                response.WriteAgentChatMessage();
            }
        }
    }

    /// <summary>
    /// 使用模板
    /// </summary>
    public async Task UseTemplateForChatCompletionAgentAsync()
    {
        var promptYaml = await File.ReadAllTextAsync("./Resources/GenerateStory.yaml");
        PromptTemplateConfig templateConfig = KernelFunctionYaml.ToPromptTemplateConfig(promptYaml);
        // 通过配置定义指令、名称和描述属性。
        ChatCompletionAgent agent = new(templateConfig, new KernelPromptTemplateFactory())
        {
            Kernel = ConfigExtensions.GetKernel("DouBao"),
            Arguments = new KernelArguments() { { "topic", "狗" }, { "length", "3" } },
        };

        /// 创建聊天历史记录以捕获代理的交互。
        ChatHistory chat = [];

        // 使用默认参数调用代理。
        await InvokeAgentAsync();

        // 使用覆盖参数调用代理。
        await InvokeAgentAsync(new() { { "topic", "猫" }, { "length", "3" } });

        // 本地函数，用于调用代理并显示对话消息。
        async Task InvokeAgentAsync(KernelArguments? arguments = null)
        {
            await foreach (ChatMessageContent content in agent.InvokeAsync(chat, arguments))
            {
                chat.Add(content);
                content.WriteAgentChatMessage();
            }
        }
    }
}
