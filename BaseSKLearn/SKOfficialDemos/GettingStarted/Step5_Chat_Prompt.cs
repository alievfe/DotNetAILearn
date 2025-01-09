using System;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

public sealed class Step5_Chat_Prompt
{
    /// <summary>
    /// 展示如何构建一个聊天提示并调用它。
    /// </summary>
    public async Task InvokeChatPromptAsync(Kernel kernel)
    {
        // 使用聊天提示调用内核，并显示结果
        string chatPrompt = """
            <message role="user">什么是西雅图？</message>
            <message role="system">以JSON格式回应。</message>
            """;

        Console.WriteLine(await kernel.InvokePromptAsync(chatPrompt));
    }
}
