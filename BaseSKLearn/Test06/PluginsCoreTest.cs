using System.Diagnostics.CodeAnalysis;
using BaseSKLearn.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

namespace BaseSKLearn;

[Experimental("SKEXP0010")]
public class PluginsCoreTest
{
    // 使用conversationSummaryPlugin中的SummarizeConversation function
    public static async Task SummarizeConversation_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        string chatTranscript = """
                                A: 你好，最近工作很忙碌，我们需要安排下周的会议时间，你觉得周几比较合适？
                                B: 嗯，我明白，工作确实很忙。周三或周四应该比较合适，因为那时候大家的日程相对空闲一些。
                                A: 好的，周三或周四都可以，我们再确认一下其他同事的时间表。
                                B: 对，最好再和大家核实一下，免得出现时间冲突。
                                A: 我今天会发邮件询问大家的意见，然后我们再做最终决定。
                                B: 好的，我也会在群里提醒大家留意邮件。

                                A: 大家好，关于下周的会议安排，我建议定在周四下午两点，在会议室A举行，大家觉得怎么样？
                                C: 周四下午两点可以，我在日历上已经标注了。
                                D: 对不起，周四下午我有其他安排，能否改到周三下午呢？
                                A: 好的，我们尽量照顾大家的时间，那就改到周三下午两点吧，地点仍然是会议室A。
                                B: 没问题，我会通知其他同事，让大家知道时间的变动。
                                """;

        var conversationSummaryPlugin = kernel.ImportPluginFromType<ConversationSummaryPlugin>();
        FunctionResult summary = await kernel.InvokeAsync(
            conversationSummaryPlugin["SummarizeConversation"],
            new() { ["input"] = chatTranscript }
        );
        Console.WriteLine($"Generated Summary:{summary}");
    }

    // 使用自定义Plugin中的SummarizeConversation function
    public static async Task My_SummarizeConversation_Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        string chatTranscript = """
                                A: 你好，最近工作很忙碌，我们需要安排下周的会议时间，你觉得周几比较合适？
                                B: 嗯，我明白，工作确实很忙。周三或周四应该比较合适，因为那时候大家的日程相对空闲一些。
                                A: 好的，周三或周四都可以，我们再确认一下其他同事的时间表。
                                B: 对，最好再和大家核实一下，免得出现时间冲突。
                                A: 我今天会发邮件询问大家的意见，然后我们再做最终决定。
                                B: 好的，我也会在群里提醒大家留意邮件。

                                A: 大家好，关于下周的会议安排，我建议定在周四下午两点，在会议室A举行，大家觉得怎么样？
                                C: 周四下午两点可以，我在日历上已经标注了。
                                D: 对不起，周四下午我有其他安排，能否改到周三下午呢？
                                A: 好的，我们尽量照顾大家的时间，那就改到周三下午两点吧，地点仍然是会议室A。
                                B: 没问题，我会通知其他同事，让大家知道时间的变动。
                                """;
        var conversationSummaryPlugin =
            kernel.ImportPluginFromType<CustomConversationSummaryPlugin>();
        FunctionResult summary = await kernel.InvokeAsync(
            conversationSummaryPlugin["SummarizeConversation"],
            new() { ["input"] = chatTranscript, ["language"] = "中文" }
        );
        Console.WriteLine($"Generated Summary:{summary}");
    }

    // 使用conversationSummaryPlugin中的GetConversationActionItems function和GetConversationTopics function
    public static async Task Test3()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("DouBao");
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        string chatTranscript = """
                                A: 你好，最近工作很忙碌，我们需要安排下周的会议时间，你觉得周几比较合适？
                                B: 嗯，我明白，工作确实很忙。周三或周四应该比较合适，因为那时候大家的日程相对空闲一些。
                                A: 好的，周三或周四都可以，我们再确认一下其他同事的时间表。
                                B: 对，最好再和大家核实一下，免得出现时间冲突。
                                A: 我今天会发邮件询问大家的意见，然后我们再做最终决定。
                                B: 好的，我也会在群里提醒大家留意邮件。

                                A: 大家好，关于下周的会议安排，我建议定在周四下午两点，在会议室A举行，大家觉得怎么样？
                                C: 周四下午两点可以，我在日历上已经标注了。
                                D: 对不起，周四下午我有其他安排，能否改到周三下午呢？
                                A: 好的，我们尽量照顾大家的时间，那就改到周三下午两点吧，地点仍然是会议室A。
                                B: 没问题，我会通知其他同事，让大家知道时间的变动。
                                """;
        
        var conversationSummaryPlugin = kernel.ImportPluginFromType<ConversationSummaryPlugin>();
        FunctionResult actionItems = await kernel.InvokeAsync(
            conversationSummaryPlugin["GetConversationActionItems"],
            new() { ["input"] = chatTranscript }
        );
        Console.WriteLine($"Generated Action Items:{actionItems}");

        FunctionResult topic = await kernel.InvokeAsync(
            conversationSummaryPlugin["GetConversationTopics"],
            new() { ["input"] = chatTranscript }
        );
        Console.WriteLine($"Generated Topics:{topic}");
    }
}
