using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SKUtils.SKExtensions;

public static class ChatExtensions
{
    /// <summary>
    /// 将格式化的代理聊天内容写入控制台的通用方法。
    /// </summary>
    public static void WriteAgentChatMessage(this ChatMessageContent message)
    {
        // 如果存在，将 ChatMessageContent.AuthorName 包含在输出中。
        string authorExpression =
            message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";

        // 如果存在，将 TextContent（通过 ChatMessageContent.Content）包含在输出中。
        string contentExpression = string.IsNullOrWhiteSpace(message.Content)
            ? string.Empty
            : message.Content;
        bool isCode =
            message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false; // 标识代码解释器内容的元数据密钥。
        string codeMarker = isCode ? "\n  [CODE]\n" : " ";
        Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");
        // 提供对非 TextContent 的内部内容的可见性。
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation) // 支持消息注释的内容类型。
            {
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {annotation.Quote}: File #{annotation.FileId}"
                );
            }
            else if (item is FileReferenceContent fileReference) // 支持文件引用的内容类型。
            {
                Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
            }
            else if (item is ImageContent image) // 表示图像内容。
            {
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}"
                );
            }
            else if (item is FunctionCallContent functionCall) // 表示AI模型请求的函数调用。
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
            }
            else if (item is FunctionResultContent functionResult) // 表示函数调用的结果。
            {
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result?.AsJson() ?? "*"}"
                );
            }
        }
    }
}
