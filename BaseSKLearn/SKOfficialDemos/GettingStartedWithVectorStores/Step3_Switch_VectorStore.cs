using Microsoft.SemanticKernel.Connectors.Redis;
using SKUtils;
using StackExchange.Redis;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithVectorStores;

/// <summary>
/// 该示例展示了可以使用相同的代码在不同的向量存储之间进行切换。
/// </summary>
public class Step3_Switch_VectorStore
{
    /// <summary>
    /// 在这里，我们将使用与 <see cref="Step1_Ingest_Data"/> 和 <see cref="Step2_Vector_Search"/> 中相同的代码，
    /// 但现在使用 <see cref="RedisVectorStore"/>。
    ///
    /// 此示例需要一个运行在 localhost:6379 的 Redis 服务器。要在 Docker 容器中运行 Redis 服务器，请使用以下命令：
    /// docker run -d --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
    /// </summary>
    [Fact]
    public async Task UseARedisVectorStoreAsync()
    {
        // 构建一个 Redis 向量存储并获取集合。
        var vectorStore = new RedisVectorStore(
            ConnectionMultiplexer.Connect("localhost:6379").GetDatabase()
        );
        var collection = vectorStore.GetCollection<string, Glossary>("skglossary");
        // 使用与步骤 1 中使用内存向量存储时相同的代码将数据摄入集合。
        await Step1_Ingest_Data.IngestDataIntoVectorStoreAsync(
            collection,
            ConfigExtensions.GetEbdService()
        );
        // 使用与步骤 2 中使用内存向量存储时相同的代码搜索向量存储。
        var searchResultItem = await Step2_Vector_Search.SearchVectorStoreAsync(
            collection,
            "什么是应用程序编程接口？",
            ConfigExtensions.GetEbdService()
        );
        // 将搜索结果及其得分输出到控制台。
        Console.WriteLine(searchResultItem.Record.Definition);
        Console.WriteLine(searchResultItem.Score);
    }
}
