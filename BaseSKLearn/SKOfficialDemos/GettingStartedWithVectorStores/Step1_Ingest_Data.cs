using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithVectorStores;

public class Step1_Ingest_Data
{
    public async Task IngestDataIntoInMemoryVectorStoreAsync()
    {
        // 构建向量存储并获取集合。
        var vectorStore = new InMemoryVectorStore();
        var collection = vectorStore.GetCollection<string, Glossary>("skglossary");
        var service = ConfigExtensions.GetEbdService("DouBao-Ebd");
        // 将数据导入到集合中。
        await IngestDataIntoVectorStoreAsync(collection, service);

        // 从集合中检索一个项目并将其输出到控制台。
        var record = await collection.GetAsync("1");
        Console.WriteLine(record!.Definition);
    }

    /// <summary>
    /// 将数据导入到给定的集合中。
    /// </summary>
    /// <param name="collection">要导入数据的集合。</param>
    /// <param name="textEmbeddingGenerationService">用于生成嵌入向量的服务。</param>
    /// <returns>插入或更新后的记录的键。</returns>
    internal static async Task<IEnumerable<string>> IngestDataIntoVectorStoreAsync(
        IVectorStoreRecordCollection<string, Glossary> collection,
        ITextEmbeddingGenerationService textEmbeddingGenerationService
    )
    {
        // 如果集合不存在，则创建它。
        await collection.CreateCollectionIfNotExistsAsync();
        // 创建词汇表条目并为它们生成嵌入向量。
        var glossaryEntries = CreateGlossaryEntries().ToList();
        var tasks = glossaryEntries.Select(entry =>
            Task.Run(async () =>
            {
                entry.DefinitionEmbedding =
                    await textEmbeddingGenerationService.GenerateEmbeddingAsync(entry.Definition);
            })
        );
        await Task.WhenAll(tasks);
        // 将词汇表条目插入或更新到集合中并返回它们的键。
        var upsertedKeysTasks = glossaryEntries.Select(x => collection.UpsertAsync(x));
        return await Task.WhenAll(upsertedKeysTasks);
    }

    /// <summary>
    /// 创建一些示例词汇表条目。
    /// </summary>
    /// <returns>示例词汇表条目的列表。</returns>
    private static IEnumerable<Glossary> CreateGlossaryEntries()
    {
        yield return new Glossary
        {
            Key = "1",
            Category = "软件",
            Term = "API",
            Definition = "应用程序编程接口。一组规则和规范，允许软件组件进行通信和交换数据。",
        };
        yield return new Glossary
        {
            Key = "2",
            Category = "软件",
            Term = "SDK",
            Definition = "软件开发工具包。一组库和工具，使软件开发人员能够更轻松地构建软件。",
        };
        yield return new Glossary
        {
            Key = "3",
            Category = "语义内核",
            Term = "连接器",
            Definition =
                "语义内核连接器允许软件开发人员与提供人工智能功能的各种服务集成，包括大语言模型、语音转文本、文本转语音、嵌入向量生成等。",
        };
        //yield return new Glossary
        //{
        //    Key = "4",
        //    Category = "语义内核",
        //    Term = "语义内核",
        //    Definition =
        //        "语义内核是一组库，使软件开发人员能够更轻松地开发利用人工智能体验的应用程序。",
        //};
        yield return new Glossary
        {
            Key = "5",
            Category = "人工智能",
            Term = "RAG",
            Definition =
                "检索增强生成 - 一个术语，指的是检索额外数据以作为上下文提供给大语言模型，以便在生成对用户问题（提示）的响应（完成结果）时使用的过程。",
        };
        yield return new Glossary
        {
            Key = "6",
            Category = "人工智能",
            Term = "大语言模型",
            Definition = "大型语言模型。一种人工智能算法，旨在理解和生成人类语言。",
        };
    }
}
