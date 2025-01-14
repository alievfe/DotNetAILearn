using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using OpenAI.Files;
using Xunit.Abstractions;
using ChatTokenUsage = OpenAI.Chat.ChatTokenUsage;

namespace SKUtils.TestUtils;

public abstract class BaseAgentsTest(ITestOutputHelper output)
    : BaseTest(output, redirectSystemConsoleOutput: true)
{
    /// <summary>
    /// 元数据键，用于指示为示例创建的助手。
    /// </summary>
    protected const string AssistantSampleMetadataKey = "sksample";

    /// <summary>
    /// 用于指示为示例创建的助手的元数据。
    /// </summary>
    /// <remarks>
    /// 尽管示例会尝试删除其创建的助手，但某些助手可能仍然存在。此元数据可用于识别和清理示例代理。
    /// </remarks>
    protected static readonly ReadOnlyDictionary<string, string> AssistantSampleMetadata = new(
        new Dictionary<string, string> { { AssistantSampleMetadataKey, bool.TrueString } }
    );

    /// <summary>
    /// 将格式化的代理聊天内容写入控制台的通用方法。
    /// </summary>
    protected void WriteAgentChatMessage(ChatMessageContent message)
    {
        // 如果存在，在输出中包含 ChatMessageContent.AuthorName。
        string authorExpression =
            message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";
        // 如果存在，在输出中包含 TextContent（通过 ChatMessageContent.Content）。
        string contentExpression = string.IsNullOrWhiteSpace(message.Content)
            ? string.Empty
            : message.Content;
        bool isCode =
            message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false;
        string codeMarker = isCode ? "\n  [CODE]\n" : " ";
        Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");

        // 遍历消息中的附加内容（如图片、文件引用、函数调用等）。提供内部内容（非 TextContent）的可见性。
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {annotation.Quote}: File #{annotation.FileId}"
                );
            }
            else if (item is FileReferenceContent fileReference)
            {
                Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
            }
            else if (item is ImageContent image)
            {
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}"
                );
            }
            else if (item is FunctionCallContent functionCall)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
            }
            else if (item is FunctionResultContent functionResult)
            {
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result?.AsJson() ?? "*"}"
                );
            }
        }

        // 如果消息中包含使用情况元数据，则显示令牌使用情况。
        if (message.Metadata?.TryGetValue("Usage", out object? usage) ?? false)
        {
            if (usage is RunStepTokenUsage assistantUsage)
            {
                WriteUsage(
                    assistantUsage.TotalTokenCount,
                    assistantUsage.InputTokenCount,
                    assistantUsage.OutputTokenCount
                );
            }
            else if (usage is ChatTokenUsage chatUsage)
            {
                WriteUsage(
                    chatUsage.TotalTokenCount,
                    chatUsage.InputTokenCount,
                    chatUsage.OutputTokenCount
                );
            }
        }

        // 本地函数，用于将令牌使用情况写入控制台。
        void WriteUsage(int totalTokens, int inputTokens, int outputTokens)
        {
            Console.WriteLine(
                $"  [Usage] Tokens: {totalTokens}, Input: {inputTokens}, Output: {outputTokens}"
            );
        }
    }

   /// <summary>
    /// 下载聊天响应中的文件内容。
    /// </summary>
    /// <param name="client">OpenAI 文件客户端。</param>
    /// <param name="message">包含文件引用的聊天消息。</param>
    /// <remarks>
    /// 此方法会遍历消息中的附加内容，下载所有注释内容引用的文件。
    /// </remarks>
    protected async Task DownloadResponseContentAsync(
        OpenAIFileClient client,
        ChatMessageContent message
    )
    {
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                await this.DownloadFileContentAsync(client, annotation.FileId!);
            }
        }
    }


    /// <summary>
    /// 下载聊天响应中的图片文件并启动查看器。
    /// </summary>
    /// <param name="client">OpenAI 文件客户端。</param>
    /// <param name="message">包含图片引用的聊天消息。</param>
    /// <remarks>
    /// 此方法会遍历消息中的附加内容，下载所有图片文件并启动默认图片查看器。
    /// </remarks>
    protected async Task DownloadResponseImageAsync(
        OpenAIFileClient client,
        ChatMessageContent message
    )
    {
        foreach (KernelContent item in message.Items)
        {
            if (item is FileReferenceContent fileReference)
            {
                await this.DownloadFileContentAsync(
                    client,
                    fileReference.FileId,
                    launchViewer: true
                );
            }
        }
    }

    /// <summary>
    /// 下载指定文件ID的文件内容。
    /// </summary>
    /// <param name="client">OpenAI 文件客户端。</param>
    /// <param name="fileId">要下载的文件ID。</param>
    /// <param name="launchViewer">是否启动文件查看器。</param>
    /// <remarks>
    /// 此方法会下载文件并将其保存到临时目录中。如果启用了查看器，则会启动默认应用程序查看文件。
    /// </remarks>
    private async Task DownloadFileContentAsync(
        OpenAIFileClient client,
        string fileId,
        bool launchViewer = false
    )
    {
        // 获取文件信息。
        OpenAIFile fileInfo = client.GetFile(fileId);
        // 检查文件是否属于助手输出。

        if (fileInfo.Purpose == FilePurpose.AssistantsOutput)
        {
            // 构建文件保存路径。
            string filePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(fileInfo.Filename));
            if (launchViewer)
            {
                // 如果启用了查看器，则将文件扩展名更改为 .png。
                filePath = Path.ChangeExtension(filePath, ".png");
            }
            // 下载文件内容并保存到本地。

            BinaryData content = await client.DownloadFileAsync(fileId);
            File.WriteAllBytes(filePath, content.ToArray());
            Console.WriteLine($"  File #{fileId} saved to: {filePath}");

            if (launchViewer)
            {
                // 启动默认应用程序查看文件。
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C start {filePath}",
                    }
                );
            }
        }
    }
}
