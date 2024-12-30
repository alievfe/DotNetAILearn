using System;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.Plugins;

public class MessageUtils
{
    [KernelFunction, Description("发送邮件")]
    public string SendEmail(string input)
    {
        Console.WriteLine(input);
        return "发送成功";
    }
}
