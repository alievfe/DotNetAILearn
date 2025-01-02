using System;
using BaseSKLearn.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos;

public class VectorStoresAndEmbeddingsTest(Kernel kernel)
{
    public async Task EmbeddingTest()
    {
        var textEmbeddingGenerationService =
            kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        var entity = new Glossary()
        {
            Key = 1,
            Term = "API",
            Definition =
                "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data.",
        };
        entity.DefinitionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(
            entity.Definition
        );
    }

    public async Task InMemoryEmbeddingTest()
    {
        // 创建向量存储库
        var vectorStore = new InMemoryVectorStore();
        // 通过向量存储库获取集合
        IVectorStoreRecordCollection<ulong, Glossary> colleciton = vectorStore.GetCollection<ulong, Glossary>("skglossary");
        // 通过直接初始化获取集合
        var colleciton2 = new InMemoryVectorStoreRecordCollection<ulong, Glossary>("skglossary");
        // 初始化集合实例将允许使用自己的集合数据，但不意味着此集合已存在于数据库中。要确保集合存在于数据库中，可以创建它：
        await colleciton.CreateCollectionIfNotExistsAsync();
        // 准备数据
        List<Glossary> glossaryEntries =
        [
            new Glossary()
            {
                Key = 1,
                Term = "API",
                Definition =
                    "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data.",
            },
            new Glossary()
            {
                Key = 2,
                Term = "Connectors",
                Definition =
                    "Connectors allow you to integrate with various services provide AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc.",
            },
            new Glossary()
            {
                Key = 3,
                Term = "RAG",
                Definition =
                    "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user's question (prompt).",
            },
        ];
        // 如果想对数据库中的记录执行向量搜索，仅初始化 key 和 data 属性是不够的，还需要生成和初始化向量属性。为此，可以使用 ITextEmbeddingGenerationService。
        var textEmbeddingGenerationService =
            kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        var tasks = glossaryEntries.Select(e =>
            Task.Run(async () =>
            {
                e.DefinitionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(
                    e.Definition
                );
            })
        );
        await Task.WhenAll(tasks);

        // 准备插入到数据库中。可以使用collection的UpsertAsync或UpsertBatchAsync 方法。此操作幂等 - 如果不存在具有特定键的记录，则会插入该记录。如果它已存在，则将对其进行更新
        await foreach (var key in colleciton.UpsertBatchAsync(glossaryEntries))
        {
            Console.WriteLine(key);
        }

        // 按键获取记录。可使用 collection.GetAsync 或 GetBatchAsync，支持GetRecordOptions作为参数，可在其中指定是否要在响应中包含向量属性。考虑到 vector 维度值可能很高，如果不需要在代码中使用 vector，建议不要从数据库中获取它们。所以 GetRecordOptions.IncludeVectors = false 是默认值。（这里为了测试所以需要看到向量）
        var options = new GetRecordOptions() { IncludeVectors = true };
        await foreach (var record in colleciton.GetBatchAsync(keys: [1, 2, 3], options))
        {
            System.Console.WriteLine(record.Definition);
            await record.SerializeObjectToFile();
        }

        // 执行搜索。要执行 vector 搜索，需要先从查询字符串生成搜索向量。
        // 然后进行搜索，使用VectorizedSearchAsync，接受 VectorSearchOptions 作为参数允许配置向量搜索操作 - 指定要返回的最大记录数、返回结果之前要跳过的结果数、在执行向量搜索之前要使用的搜索过滤器等。
        var searchString = "I want to learn more about Connectors";
        var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(
            searchString
        );

        var searchResult = await colleciton.VectorizedSearchAsync(searchVector);
        await foreach (var result in searchResult.Results)
        {
            Console.WriteLine($"Search score: {result.Score}");
            Console.WriteLine($"Key: {result.Record.Key}");
            Console.WriteLine($"Term: {result.Record.Term}");
            Console.WriteLine($"Definition: {result.Record.Definition}");
            Console.WriteLine("=========");
        }
    }
}
