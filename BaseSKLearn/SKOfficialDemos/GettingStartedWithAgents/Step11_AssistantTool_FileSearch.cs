using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Files;
using OpenAI.VectorStores;
using Resources;
using SKUtils.TestUtils;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;

/// <summary>
/// 演示如何在 <see cref="OpenAIAssistantAgent"/> 上使用文件搜索工具。
/// </summary>
public class Step11_AssistantTool_FileSearch(ITestOutputHelper output) : BaseAgentsTest(output)
{
    [Fact]
    public async Task UseFileSearchToolWithAssistantAgentAsync()
    {
        // 定义代理
        var (provider, modelId) = GetClientProvider();
        OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(
            clientProvider: provider,
            definition: new(modelId)
            {
                EnableCodeInterpreter = true, // 启用代码解释器工具
                Metadata = AssistantSampleMetadata,
            },
            kernel: new Kernel()
        );

        // 上传文件 - 使用一个虚构的员工表格。
        OpenAIFileClient fileClient = provider.Client.GetOpenAIFileClient();
        await using Stream stream = EmbeddedResource.ReadStream("employees.pdf")!;
        OpenAIFile fileInfo = await fileClient.UploadFileAsync(
            stream,
            "employees.pdf",
            FileUploadPurpose.Assistants
        );

        // 创建向量存储
        VectorStoreClient vectorStoreClient = provider.Client.GetVectorStoreClient();
        CreateVectorStoreOperation result = await vectorStoreClient.CreateVectorStoreAsync(
            waitUntilCompleted: false,
            new VectorStoreCreationOptions()
            {
                FileIds = { fileInfo.Id }, // 将上传的文件 ID 添加到向量存储
                Metadata = { { AssistantSampleMetadataKey, bool.TrueString } }, // 添加元数据
            }
        );

        // 为代理对话创建一个与向量存储关联的线程。
        string threadId = await agent.CreateThreadAsync(
            new OpenAIThreadCreationOptions
            {
                VectorStoreId = result.VectorStoreId, // 关联向量存储 ID
                Metadata = AssistantSampleMetadata,
            }
        );

        // 响应用户输入
        try
        {
            // 调用代理并询问问题
            await InvokeAgentAsync("Who is the youngest employee?");
            await InvokeAgentAsync("Who works in sales?");
            await InvokeAgentAsync("I have a customer request, who can help me?");
        }
        finally
        {
            // 清理资源：删除线程、代理、向量存储和上传的文件。
            await agent.DeleteThreadAsync(threadId);
            await agent.DeleteAsync();
            await vectorStoreClient.DeleteVectorStoreAsync(result.VectorStoreId);
            await fileClient.DeleteFileAsync(fileInfo.Id);
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
