using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Text;

namespace BaseSKLearn;

[Experimental("SKEXP0050")]
public class CustomConversationSummaryPlugin
{
    private const int MaxTokens = 1024;
    private readonly KernelFunction _summarizeConversationFunction;
    internal const string SummarizeConversationDefinition = """
                                                            BEGIN CONTENT TO SUMMARIZE:
                                                            {{$INPUT}}

                                                            END CONTENT TO SUMMARIZE.

                                                            Please summarize the conversation, highlighting the main points and any conclusions reached, in {{$LANGUAGE}} if specified, or in the most appropriate language otherwise.
                                                            Do not incorporate any external general knowledge.
                                                            The summary should be in plain text, in complete sentences, without any markup or tags.

                                                            BEGIN SUMMARY:
                                                            """;

    public CustomConversationSummaryPlugin()
    {
        PromptExecutionSettings executionSettings = new PromptExecutionSettings()
        {
            ExtensionData = new Dictionary<string, object>()
            {
                { "Temperature", 0.1 },
                { "TopP", 0.5 },
                { nameof(MaxTokens), 1024 }
            }
        };
        _summarizeConversationFunction = KernelFunctionFactory.CreateFromPrompt(
            SummarizeConversationDefinition,
            executionSettings,
            description: "Given a section of a conversation transcript, summarize the part of the conversation."
        );
    }

    /// <summary>
    /// Given a long conversation transcript, summarize the conversation.
    /// </summary>
    /// <param name="input">A long conversation transcript.</param>
    /// <param name="kernel">The <see cref="T:Microsoft.SemanticKernel.Kernel" /> containing services, plugins, and other state for use throughout the operation.</param>
    /// <param name="language">Mandatory specified language for summarizing</param>
    [
        KernelFunction,
        Description("Given a long conversation transcript, summarize the conversation.")
    ]
    public async Task<string> SummarizeConversationAsync(
        Kernel kernel,
        [Description("A long conversation transcript.")] string input,
        [Description("Mandatory specified language for summarizing")] string? language = null
    )
    {
        // 文本分块器，每段落1024token，每行1024token
        var paragraphs = TextChunker.SplitPlainTextParagraphs(
            TextChunker.SplitPlainTextLines(input, 1024),
            1024
        );
        var results = new string[paragraphs.Count];
        for (var i = 0; i < results.Length; i++)
        {
            // The first parameter is the input text.
            results[i] =
                (
                    await _summarizeConversationFunction
                        .InvokeAsync(
                            kernel,
                            new() { ["input"] = paragraphs[i], ["language"] = language }
                        )
                        .ConfigureAwait(false)
                ).GetValue<string>() ?? string.Empty;
        }
        return string.Join("\n", results);
    }
}
