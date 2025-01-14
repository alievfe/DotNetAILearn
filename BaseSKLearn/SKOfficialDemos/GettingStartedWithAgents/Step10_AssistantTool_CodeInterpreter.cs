using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using SKUtils.TestUtils;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示如何在 <see cref="OpenAIAssistantAgent"/> 上使用代码解释器工具。
/// </summary>
public class Step10_AssistantTool_CodeInterpreter(ITestOutputHelper output) : BaseAgentsTest(output)
{
    [Fact]
    public async Task UseCodeInterpreterToolWithAssistantAgentAsync()
    {
        var (provider, modelId) = GetClientProvider();

        // 定义代理
        OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(
            clientProvider: provider,
            definition: new(modelId)
            {
                EnableCodeInterpreter = true, // 启用代码解释器工具
                Metadata = AssistantSampleMetadata,
            },
            kernel: new Kernel()
        );

        // 为代理对话创建一个线程。
        string threadId = await agent.CreateThreadAsync(
            new OpenAIThreadCreationOptions { Metadata = AssistantSampleMetadata }
        );
        
        // 响应用户输入
        try
        {
            // 调用代理并询问问题
            await InvokeAgentAsync("使用代码确定斐波那契数列中小于 101 的值有哪些？");
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
            this.WriteAgentChatMessage(message);

            await foreach (ChatMessageContent response in agent.InvokeAsync(threadId))
            {
                this.WriteAgentChatMessage(response);
            }
        }
    }
}
