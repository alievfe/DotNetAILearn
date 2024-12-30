using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
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
                    var chatConfig = ConfigExtensions.GetConfig<OpenAIConfig>(
                        "./tmpsecrets.json",
                        "DouBao"
                    );
                    var ebdConfig = ConfigExtensions.GetConfig<OpenAIConfig>(
                        "./tmpsecrets.json",
                        "DouBao-Ebd"
                    );

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

                    services.AddSqliteVectorStore("Data Source=E:/Develop/SqliteData/MyApp.db");

                    services.AddHostedService<Worker>();
                }
            )
            .RunConsoleAsync();
    }
}

internal class Worker(
    IHostApplicationLifetime hostLifetime,
    Kernel kernel,
    IVectorStore vectorStore,
    ITextEmbeddingGenerationService ebdService
) : IHostedService
{
    private int? _exitCode;

    public async Task TestStore()
    {
        var colleciton = vectorStore.GetCollection<ulong, BiliVideo>("bilivideo");
        await colleciton.CreateCollectionIfNotExistsAsync();
        // var data = BiliBiliData();
        // var tasks = data.Select(e =>
        //     Task.Run(async () =>
        //     {
        //         e.TitleEmbedding = await ebdService.GenerateEmbeddingAsync(e.Title);
        //     })
        // );
        // await Task.WhenAll(tasks);
        // await foreach (var key in colleciton.UpsertBatchAsync(data))
        // {
        //     Console.WriteLine(key);
        // }
        // var options = new GetRecordOptions() { IncludeVectors = true };
        // await foreach (var record in colleciton.GetBatchAsync(keys: [1, 2, 3], options))
        // {
        //     System.Console.WriteLine(JsonSerializer.Serialize(record));
        // }
    }

    private List<BiliVideo> BiliBiliData() =>
        [
            new()
            {
                Key = 1,
                Link = "https://www.bilibili.com/video/BV1sr4y1f7zb/",
                Title = "SK 插件Plugins及VSCode调试工具",
            },
            new()
            {
                Key = 2,
                Link = "https://www.bilibili.com/video/BV1Hw411Y71S",
                Title = "SK 原生函数使用方法",
            },
            new()
            {
                Key = 3,
                Link = "https://www.bilibili.com/video/BV1zF411m7YA/",
                Title = "SK 嵌套函数使用方法",
            },
            new()
            {
                Key = 4,
                Link = "https://www.bilibili.com/video/BV1F841117Jc/",
                Title = "SK 原生函数及嵌套函数串联使用方法",
            },
        ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            //
            await TestStore();
            _exitCode = 0;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
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
    public ReadOnlyMemory<float>? TitleEmbedding { get; set; }
}
