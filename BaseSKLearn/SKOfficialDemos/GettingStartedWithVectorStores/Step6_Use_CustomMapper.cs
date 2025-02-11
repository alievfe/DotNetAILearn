using System.Text.Json;
using System.Text.Json.Nodes;
using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using SKUtils;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithVectorStores;

/// <summary>
/// 该示例展示了如果希望数据模型和存储模式不同，如何使用自定义映射器。
/// </summary>
public class Step6_Use_CustomMapper
{
    /// <summary>
    /// 此示例展示了在希望数据模型和存储模式不同时，使用自定义映射器插入和查询记录的方法。
    ///
    /// 此示例需要一个可用的 Azure AI 搜索服务。
    /// </summary>
    public async Task UseCustomMapperAsync()
    {
        var ebdsvc = ConfigExtensions.GetEbdService();
        // 使用自定义映射器时，我们仍然需要使用记录定义向向量存储描述存储模式。
        // 由于存储模式与数据模型不匹配，向量存储无法从数据模型推断出模式。
        var recordDefinition = new VectorStoreRecordDefinition
        {
            Properties = new List<VectorStoreRecordProperty>
            {
                new VectorStoreRecordKeyProperty("Key", typeof(string)),
                new VectorStoreRecordDataProperty("Category", typeof(string)),
                new VectorStoreRecordDataProperty("Term", typeof(string)),
                new VectorStoreRecordDataProperty("Definition", typeof(string)),
                new VectorStoreRecordVectorProperty(
                    "DefinitionEmbedding",
                    typeof(ReadOnlyMemory<float>)
                )
                {
                    Dimensions = 1536,
                },
            },
        };
        // 构建一个 Azure AI 搜索向量存储集合，
        // 并传入自定义映射器和记录定义。
        var collection = new AzureAISearchVectorStoreRecordCollection<ComplexGlossary>(
            new SearchIndexClient(new Uri(""), new AzureKeyCredential("")),
            "skglossary",
            new()
            {
                JsonObjectCustomMapper = new CustomMapper(),
                VectorStoreRecordDefinition = recordDefinition,
            }
        );
        // 如果集合不存在，则创建它。
        // 此调用将使用上面记录定义中定义的模式来创建集合。
        await collection.CreateCollectionIfNotExistsAsync();
        // 现在我们可以使用数据模型插入一条记录，即使它与存储模式不匹配。
        var definition = "一组规则和协议，允许一个软件应用程序与另一个进行交互。";
        await collection.UpsertAsync(
            new ComplexGlossary
            {
                Key = "1",
                Metadata = new Metadata { Category = "API", Term = "应用程序编程接口" },
                Definition = definition,
                DefinitionEmbedding = await ebdsvc.GenerateEmbeddingAsync(definition),
            }
        );
        // 从搜索字符串生成嵌入向量。
        var searchVector = await ebdsvc.GenerateEmbeddingAsync("两个软件应用程序如何相互交互？");
        // 搜索向量存储。
        var searchResult = await collection.VectorizedSearchAsync(searchVector, new() { Top = 1 });
        var searchResultItem = await searchResult.Results.FirstAsync();
        // 将搜索结果及其得分输出到控制台。
        Console.WriteLine(searchResultItem.Record.Metadata.Term);
        Console.WriteLine(searchResultItem.Record.Definition);
        Console.WriteLine(searchResultItem.Score);
    }

    /// <summary>
    /// 示例映射器类，用于在 <see cref="ComplexGlossary"/> 自定义数据模型
    /// 和应与存储模式匹配的 <see cref="JsonObject"/> 之间进行映射。
    /// </summary>
    private sealed class CustomMapper : IVectorStoreRecordMapper<ComplexGlossary, JsonObject>
    {
        public JsonObject MapFromDataToStorageModel(ComplexGlossary dataModel)
        {
            return new JsonObject
            {
                ["Key"] = dataModel.Key,
                ["Category"] = dataModel.Metadata.Category,
                ["Term"] = dataModel.Metadata.Term,
                ["Definition"] = dataModel.Definition,
                ["DefinitionEmbedding"] = JsonSerializer.SerializeToNode(
                    dataModel.DefinitionEmbedding.ToArray()
                ),
            };
        }

        public ComplexGlossary MapFromStorageToDataModel(
            JsonObject storageModel,
            StorageToDataModelMapperOptions options
        )
        {
            return new ComplexGlossary
            {
                Key = storageModel["Key"]!.ToString(),
                Metadata = new Metadata
                {
                    Category = storageModel["Category"]!.ToString(),
                    Term = storageModel["Term"]!.ToString(),
                },
                Definition = storageModel["Definition"]!.ToString(),
                DefinitionEmbedding = JsonSerializer.Deserialize<ReadOnlyMemory<float>>(
                    storageModel["DefinitionEmbedding"]
                ),
            };
        }
    }

    /// <summary>
    /// 示例模型类，代表一个术语表条目。
    /// 此模型与之前步骤中使用的模型不同，它有一个复杂属性 <see cref="Metadata"/>，其中包含类别和术语。
    /// </summary>
    private sealed class ComplexGlossary
    {
        public string Key { get; set; }
        public Metadata Metadata { get; set; }
        public string Definition { get; set; }
        public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
    }

    private sealed class Metadata
    {
        public string Category { get; set; }
        public string Term { get; set; }
    }
}
