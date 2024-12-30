using System.Text.RegularExpressions;

namespace SKUtils;

/// <summary>
/// 用于请求重定向到自定义的endpoint
/// </summary>
public partial class AIHostCustomHandler : HttpClientHandler
{
    private readonly string _baseUrl;
    private readonly string _complectionPath = "/chat/completions";
    private readonly string _embeddingPath = "/embeddings";

    [GeneratedRegex(@"/v\d$")]
    private static partial Regex VersionPattern();

    private static readonly string[] UrlSources = ["api.openai.com", "openai.azure.com"];

    /// <summary>
    /// 使用指定的模型URL初始化<see cref="AIHostCustomHandler"/>类的新实例。
    /// </summary>
    /// <param name="baseUrl">用于OpenAI或Azure OpenAI请求的基础URL。</param>
    public AIHostCustomHandler(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("模型URL不能为空或空白。", nameof(baseUrl));
        // 检查是否末尾携带了版本号，如果没有则默认加上 /v1
        if (!VersionPattern().IsMatch(baseUrl))
        {
            baseUrl += "/v1";
        }
        _baseUrl = baseUrl;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // 替换请求路径
        if (request.RequestUri is not null && UrlSources.Contains(request.RequestUri.Host))
        {
            if (request.RequestUri.PathAndQuery.EndsWith("completions"))
            {
                request.RequestUri = new Uri(_baseUrl + _complectionPath);
            }
            else if (request.RequestUri.PathAndQuery.EndsWith("embeddings"))
            {
                request.RequestUri = new Uri(_baseUrl + _embeddingPath);
            }
            else
            {
                throw new NotImplementedException($"未实现此分支。请求路径：{request.RequestUri}");
            }
        }

        // Log the request URI and method
        Console.WriteLine($"Request: {request.Method} {request.RequestUri}");

        // Check if it's a POST request with JSON content
        if (request.Method == HttpMethod.Post && request.Content != null)
        {
            // Get the Bearer token from the Authorization header
            if (request.Headers.Authorization?.Scheme == "Bearer")
            {
                Console.WriteLine($"Bearer Token: {request.Headers.Authorization.Parameter}");
            }

            // Read the content of the POST request as a string
            var requestBody = await request.Content.ReadAsStringAsync();
            Console.WriteLine($"Request Body: {requestBody}");
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        // Read the content of the POST response as a string
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Body: {responseBody}");
        return response;
    }
}
