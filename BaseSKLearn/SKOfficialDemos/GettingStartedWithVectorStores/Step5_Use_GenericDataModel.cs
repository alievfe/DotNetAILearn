using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.SemanticKernel.Embeddings;
using SKUtils;
using StackExchange.Redis;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithVectorStores;

/// <summary>
/// 该示例展示了可以使用通用数据模型与向量数据库进行交互。
/// 这使得在无需创建自己的数据模型的情况下，也能使用向量存储抽象。
/// </summary>
public class Step5_Use_GenericDataModel
{
    /// <summary>
    /// 此示例展示了如何查询使用通用数据模型的向量存储。
    /// 此示例需要一个运行在 localhost:6379 的 Redis 服务器。要在 Docker 容器中运行 Redis 服务器，请使用以下命令：
    /// docker run -d --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
    /// </summary>
    [Fact]
    public async Task SearchAVectorStoreWithGenericDataModelAsync()
    {
        var ebdsvc = ConfigExtensions.GetEbdService();
        // 构建一个 Redis 向量存储。
        var vectorStore = new RedisVectorStore(
            ConnectionMultiplexer.Connect("localhost:6379").GetDatabase()
        );
        // 首先，使用步骤 1 中的代码将数据摄入向量存储，
        // 使用自定义数据模型，模拟之前由其他人将数据摄入数据库的场景。
        var collection = vectorStore.GetCollection<string, Glossary>("skglossary");
        var customDataModelCollection = vectorStore.GetCollection<string, Glossary>("skglossary");
        await Step1_Ingest_Data.IngestDataIntoVectorStoreAsync(customDataModelCollection, ebdsvc);

        // 要使用通用数据模型，仍然需要使用记录定义向向量存储描述存储模式。
        // 与自定义数据模型相比，这种定义不必在编译时确定。
        // 例如，它可以从配置中读取或从服务中获取。
        var recordDefinition = new VectorStoreRecordDefinition
        {
            Properties =
            [
                new VectorStoreRecordKeyProperty("Key", typeof(string)),
                new VectorStoreRecordDataProperty("Category", typeof(string)),
                new VectorStoreRecordDataProperty("Term", typeof(string)),
                new VectorStoreRecordDataProperty("Definition", typeof(string)),
                new VectorStoreRecordVectorProperty(
                    "DefinitionEmbedding",
                    typeof(ReadOnlyMemory<float>)
                )
                {
                    Dimensions = 2560,
                },
            ],
        };
        // 现在，创建一个使用通用数据模型的集合。
        var genericDataModelCollection = vectorStore.GetCollection<
            string,
            VectorStoreGenericDataModel<string>
        >("skglossary", recordDefinition);
        // 从搜索字符串生成嵌入向量。
        var searchString = "如何为大语言模型提供额外的上下文信息？";
        var searchVector = await ebdsvc.GenerateEmbeddingAsync(searchString);
        // 搜索通用数据模型集合并获取最相关的单个结果。
        var searchResult = await genericDataModelCollection.VectorizedSearchAsync(
            searchVector,
            new() { Top = 1 }
        );
        var searchResultItems = await searchResult.Results.ToListAsync();
        // 将搜索结果及其得分输出到控制台。
        // 注意，这里可以遍历所有数据属性，而无需了解模式，因为使用通用数据模型时，数据属性以字符串键和对象值的字典形式存储。
        foreach (var dataProperty in searchResultItems.First().Record.Data)
        {
            Console.WriteLine($"{dataProperty.Key}: {dataProperty.Value}");
        }
        Console.WriteLine(searchResultItems.First().Score);
    }
}
