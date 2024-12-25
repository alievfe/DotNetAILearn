namespace SKUtils;


/// <summary>
/// 用于请求重定向到自定义的endpoint
/// </summary>
public class OneAPICustomHandler : HttpClientHandler
{
    private readonly string _host;
    private static readonly string[] UrlSources = ["api.openai.com", "openai.azure.com"];

    /// <summary>
    /// 使用指定的模型URL初始化<see cref="OneAPICustomHandler"/>类的新实例。
    /// </summary>
    /// <param name="host">用于OpenAI或Azure OpenAI请求的基础URL。</param>
    public OneAPICustomHandler(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("模型URL不能为空或空白。", nameof(host));
        _host = host;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // 替换请求路径的host
        if (request.RequestUri is not null && UrlSources.Contains(request.RequestUri.Host))
        {
            request.RequestUri = new Uri(_host + request.RequestUri.PathAndQuery);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
