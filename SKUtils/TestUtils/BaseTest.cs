using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit.Abstractions;

namespace SKUtils.TestUtils;

public abstract class BaseTest : TextWriter
{
    protected ITestOutputHelper Output { get; }

    protected ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// 此属性使示例对控制台友好。允许它们被复制并粘贴到控制台应用程序中，只需进行少量更改。
    /// </summary>
    public BaseTest Console => this;

    protected BaseTest(ITestOutputHelper output, bool redirectSystemConsoleOutput = false)
    {
        this.Output = output;
        this.LoggerFactory = new XunitLogger(output);

        // 如果请求，将 System.Console 输出重定向到测试输出
        if (redirectSystemConsoleOutput)
        {
            System.Console.SetOut(this);
        }
    }

    /// <inheritdoc/>
    public override void WriteLine(object? value = null) =>
        this.Output.WriteLine(value?.ToString() ?? string.Empty);

    /// <inheritdoc/>
    public override void WriteLine(string? format, params object?[] arg) =>
        this.Output.WriteLine(format ?? string.Empty, arg);

    /// <inheritdoc/>
    public override void WriteLine(string? value) => this.Output.WriteLine(value ?? string.Empty);

    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="ITestOutputHelper"/> only supports output that ends with a newline.
    /// User this method will resolve in a call to <see cref="WriteLine(string?)"/>.
    /// </remarks>
    public override void Write(object? value = null) =>
        this.Output.WriteLine(value?.ToString() ?? string.Empty);

    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="ITestOutputHelper"/> only supports output that ends with a newline.
    /// User this method will resolve in a call to <see cref="WriteLine(string?)"/>.
    /// </remarks>
    public override void Write(char[]? buffer) => this.Output.WriteLine(new string(buffer));

    /// <inheritdoc/>
    public override Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// 输出聊天历史中的最后一条消息。
    /// </summary>
    /// <param name="chatHistory">Chat history</param>
    protected void OutputLastMessage(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");
    }

    /// <summary>
    /// 将水平规则写入控制台的实用方法。
    /// </summary>
    protected void WriteHorizontalRule() =>
        Console.WriteLine(new string('-', HorizontalRuleLength));

    protected sealed class LoggingHandler(HttpMessageHandler innerHandler, ITestOutputHelper output)
        : DelegatingHandler(innerHandler)
    {
        private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
        {
            WriteIndented = true,
        };

        private readonly ITestOutputHelper _output = output;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            // Log the request details
            if (request.Content is not null)
            {
                var content = await request.Content.ReadAsStringAsync(cancellationToken);
                this._output.WriteLine("=== REQUEST ===");
                try
                {
                    string formattedContent = JsonSerializer.Serialize(
                        JsonSerializer.Deserialize<JsonElement>(content),
                        s_jsonSerializerOptions
                    );
                    this._output.WriteLine(formattedContent);
                }
                catch (JsonException)
                {
                    this._output.WriteLine(content);
                }
                this._output.WriteLine(string.Empty);
            }

            // Call the next handler in the pipeline
            var response = await base.SendAsync(request, cancellationToken);

            if (response.Content is not null)
            {
                // Log the response details
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                this._output.WriteLine("=== RESPONSE ===");
                this._output.WriteLine(responseContent);
                this._output.WriteLine(string.Empty);
            }

            return response;
        }
    }

    #region private
    private const int HorizontalRuleLength = 80;
    #endregion
}
