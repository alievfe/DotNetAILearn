using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using SKUtils;

namespace BaseSKLearn.XZYDemos;

public class SKMemoryXZYTest
{
    public static async Task DI()
    {
        await Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder => { })
            .ConfigureServices(
                (hostContext, services) =>
                {
                    var chatConfig = ConfigExtensions.GetConfig<OpenAIConfig, Program>("DouBao");
                    var ebdConfig = ConfigExtensions.GetConfig<OpenAIConfig, Program>("DouBao-Ebd");

                    // 注册kernel
                    services
                        .AddKernel()
                        .AddOpenAIChatCompletion(
                            modelId: chatConfig.ModelId,
                            apiKey: chatConfig.ApiKey,
                            endpoint: chatConfig.Endpoint
                        )
                        .AddOpenAITextEmbeddingGeneration(
                            modelId: ebdConfig.ModelId,
                            apiKey: ebdConfig.ApiKey,
                            httpClient: new HttpClient(new AIHostCustomHandler(ebdConfig.Host))
                        );

                    services.AddSqliteVectorStore("Data Source=:memory:");

                    services.AddHostedService<Worker>();
                }
            )
            .RunConsoleAsync();
    }

    private Dictionary<string, string> BiliBiliData()
    {
        return new Dictionary<string, string>
        {
            ["https://www.bilibili.com/video/BV1sr4y1f7zb/"] = "SK 插件Plugins及VSCode调试工具",
            ["https://www.bilibili.com/video/BV1Hw411Y71S"] = "SK 原生函数使用方法",
            ["https://www.bilibili.com/video/BV1zF411m7YA/"] = "SK 嵌套函数使用方法",
            ["https://www.bilibili.com/video/BV1F841117Jc/"] = "SK 原生函数及嵌套函数串联使用方法",
            ["https://www.bilibili.com/video/BV12j41187GX/"] = "SK Plan流程编排",
            ["https://www.bilibili.com/video/BV1nm4y1V7dz/"] = "SK 意图识别、json提取",
            ["https://www.bilibili.com/video/BV1Qj41147i6/"] = "SK 依赖注入、Pipeline",
        };
    }
}

internal class Worker(
    IHostApplicationLifetime hostLifetime,
    Kernel kernel,
    IVectorStore vectorStore
) : IHostedService
{
    private int? _exitCode;

    public async Task TestStore() 
    { 
        var colleciton = vectorStore.GetCollection<ulong, BiliVideo>("bilivideo");
        await colleciton.CreateCollectionIfNotExistsAsync();
        colleciton.UpsertBatchAsync
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            //
            _exitCode = 0;
        }
        catch (Exception)
        {
            _exitCode = 1;
        }
        finally
        {
            hostLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
        return Task.CompletedTask;
    }
}

internal class BiliVideo
{
    [VectorStoreRecordKey]
    public ulong Key { get; set; }

    [VectorStoreRecordData]
    public string Link { get; set; }

    [VectorStoreRecordData]
    public string Title { get; set; }

    [VectorStoreRecordVector(Dimensions: 2560)]
    public ReadOnlyMemory<float> TitleEmbedding { get; set; }
}
