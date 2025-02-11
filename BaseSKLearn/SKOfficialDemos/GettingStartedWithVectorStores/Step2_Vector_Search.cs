using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using SKUtils;
using SKUtils.TestUtils;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithVectorStores;

/// <summary>
/// 该示例展示了如何使用内存向量存储进行向量搜索。
/// </summary>
public class Step2_Vector_Search
{
    /// <summary>
    /// 执行基本的向量搜索，仅检索最相关的单个结果。
    /// </summary>
    [Fact]
    public async Task SearchAnInMemoryVectorStoreAsync()
    {
        var collection = await GetVectorStoreCollectionWithDataAsync();
        // 搜索向量存储。
        var searchResultItem = await SearchVectorStoreAsync(
            collection,
            "什么是应用程序编程接口？",
            ConfigExtensions.GetEbdService()
        );
        // 将搜索结果及其得分输出到控制台。
        Console.WriteLine(searchResultItem.Record.Definition);
        Console.WriteLine(searchResultItem.Score);
    }

    /// <summary>
    /// 在给定的集合中搜索与给定搜索字符串最相关的结果。
    /// </summary>
    /// <param name="collection">要搜索的集合。</param>
    /// <param name="searchString">要匹配的搜索字符串。</param>
    /// <param name="textEmbeddingGenerationService">用于生成嵌入向量的服务。</param>
    /// <returns>排名最高的搜索结果。</returns>
    internal static async Task<VectorSearchResult<Glossary>> SearchVectorStoreAsync(
        IVectorStoreRecordCollection<string, Glossary> collection,
        string searchString,
        ITextEmbeddingGenerationService textEmbeddingGenerationService
    )
    {
        // 从搜索字符串生成嵌入向量。
        var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(
            searchString
        );
        // 搜索存储并获取最相关的单个结果。
        var searchResult = await collection.VectorizedSearchAsync(searchVector, new() { Top = 1 });
        var searchResultItems = await searchResult.Results.ToListAsync();
        return searchResultItems.First();
    }

    /// <summary>
    /// 执行带有预过滤的更复杂的向量搜索。
    /// </summary>
    [Fact]
    public async Task SearchAnInMemoryVectorStoreWithFilteringAsync()
    {
        var collection = await GetVectorStoreCollectionWithDataAsync();
        // 从搜索字符串生成嵌入向量。
        var searchString = "如何为大语言模型提供额外的上下文信息？";
        var searchVector = await ConfigExtensions
            .GetEbdService()
            .GenerateEmbeddingAsync(searchString);
        // 使用过滤器搜索存储并获取最相关的单个结果。
        var searchResult = await collection.VectorizedSearchAsync(
            searchVector,
            new()
            {
                Top = 1,
                Filter = new VectorSearchFilter().EqualTo(nameof(Glossary.Category), "人工智能"),
            }
        );
        var searchResultItems = await searchResult.Results.ToListAsync();
        // 将搜索结果及其得分输出到控制台。
        Console.WriteLine(searchResultItems.First().Record.Definition);
        Console.WriteLine(searchResultItems.First().Score);
    }

    private async Task<
        IVectorStoreRecordCollection<string, Glossary>
    > GetVectorStoreCollectionWithDataAsync()
    {
        // 构建向量存储并获取集合。
        var vectorStore = new InMemoryVectorStore();
        var collection = vectorStore.GetCollection<string, Glossary>("skglossary");
        // 使用步骤 1 中的代码将数据摄入集合。
        await Step1_Ingest_Data.IngestDataIntoVectorStoreAsync(
            collection,
            ConfigExtensions.GetEbdService()
        );
        return collection;
    }
}
