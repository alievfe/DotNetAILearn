using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Resources;
using SKUtils.TestUtils;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示如何向 <see cref="OpenAIAssistantAgent"/> 提供图像输入。
/// </summary>
public class Step09_Assistant_Vision(ITestOutputHelper output) : BaseAgentsTest(output)
{
    [Fact]
    public async Task UseSingleAssistantAgentAsync()
    {
        var (provider, modelId) = GetClientProvider();
        // 定义代理
        OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(
            provider,
            definition: new OpenAIAssistantDefinition(modelId)
            {
                Metadata = AssistantSampleMetadata,
            },
            kernel: new Kernel()
        );

        // 上传一张图片
        await using Stream imageStream = EmbeddedResource.ReadStream("cat.jpg")!;
        string fileId = await agent.UploadFileAsync(imageStream, "cat.jpg");

        // 为代理对话创建一个线程。
        string threadId = await agent.CreateThreadAsync(
            new OpenAIThreadCreationOptions { Metadata = AssistantSampleMetadata }
        );

        // 响应用户输入
        try
        {
            // 通过 URL 引用公共图片
            await InvokeAgentAsync(
                CreateMessageWithImageUrl(
                    "描述这张图片。",
                    "https://upload.wikimedia.org/wikipedia/commons/thumb/4/47/New_york_times_square-terabass.jpg/1200px-New_york_times_square-terabass.jpg"
                )
            );
            await InvokeAgentAsync(
                CreateMessageWithImageUrl(
                    "这张图片的主色调是什么？",
                    "https://upload.wikimedia.org/wikipedia/commons/5/56/White_shark.jpg"
                )
            );
            // 通过文件 ID 引用上传的图片。
            await InvokeAgentAsync(CreateMessageWithImageReference("这张图片中有动物吗？", fileId));
        }
        finally
        {
            // 清理资源：删除线程、代理和上传的文件。
            await agent.DeleteThreadAsync(threadId);
            await agent.DeleteAsync();
            await provider.Client.GetOpenAIFileClient().DeleteFileAsync(fileId);
        }

        // 本地函数，用于调用代理并显示对话消息。
        async Task InvokeAgentAsync(ChatMessageContent message)
        {
            await agent.AddChatMessageAsync(threadId, message);
            this.WriteAgentChatMessage(message);

            await foreach (ChatMessageContent response in agent.InvokeAsync(threadId))
            {
                this.WriteAgentChatMessage(response);
            }
        }
    }

    /// <summary>
    /// 创建一个包含图片 URL 的聊天消息。
    /// </summary>
    /// /// <param name="input">用户输入的文本。</param>
    /// <param name="url">图片的 URL。</param>
    /// <returns>包含文本和图片 URL 的聊天消息。</returns>
    private ChatMessageContent CreateMessageWithImageUrl(string input, string url) =>
        new(AuthorRole.User, [new TextContent(input), new ImageContent(new Uri(url))]);

    /// <summary>
    /// 创建一个包含图片文件引用的聊天消息。
    /// </summary>
    /// <param name="input">用户输入的文本。</param>
    /// <param name="fileId">上传图片的文件 ID。</param>
    /// <returns>包含文本和图片文件引用的聊天消息。</returns>
    private ChatMessageContent CreateMessageWithImageReference(string input, string fileId) =>
        new(AuthorRole.User, [new TextContent(input), new FileReferenceContent(fileId)]);
}
