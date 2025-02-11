using System.Runtime.CompilerServices;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithVectorStores;

/// <summary>
/// 该示例展示了可以使用相同的代码在不同的向量存储之间进行切换，在这种情况下，
/// 所使用的向量存储不使用字符串键。
/// 此示例演示了一种可行的方法，不过也可以在通用代码中使用泛型来实现代码复用。
/// </summary>
public class Step4_NonStringKey_VectorStore
{
    /// <summary>
    /// 在这里，我们将使用与 <see cref="Step1_Ingest_Data"/> 和 <see cref="Step2_Vector_Search"/> 中相同的代码，
    /// 但现在使用 <see cref="QdrantVectorStore"/>。
    /// Qdrant 使用 Guid 或 ulong 作为键类型，但通用代码使用字符串键。不过，在 <see cref="Step1_Ingest_Data"/> 中创建的记录的字符串键包含数字，
    /// 因此我们可以将它们转换为 ulong。
    /// 在这个示例中，我们将演示如何进行这种转换。
    ///
    /// 此示例需要一个正在运行的 Qdrant 服务器。要在 Docker 容器中运行 Qdrant 服务器，请使用以下命令：
    /// docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant:latest
    /// </summary>
    public async Task UseAQdrantVectorStoreAsync()
    {
        var ebdsvc = ConfigExtensions.GetEbdService();
        var collection = new QdrantVectorStoreRecordCollection<UlongGlossary>(
            new QdrantClient("localhost"),
            "skglossary"
        );
        // 使用一个装饰器包装集合，该装饰器允许我们公开一个使用字符串键的版本，但在内部进行 ulong 与字符串之间的转换。
        var stringKeyCollection = new MappingVectorStoreRecordCollection<
            string,
            ulong,
            Glossary,
            UlongGlossary
        >(
            collection,
            ulong.Parse,
            e => e.ToString(),
            e => new UlongGlossary
            {
                Key = ulong.Parse(e.Key),
                Category = e.Category,
                Term = e.Term,
                Definition = e.Definition,
                DefinitionEmbedding = e.DefinitionEmbedding,
            },
            e => new Glossary
            {
                Key = e.Key.ToString("D"),
                Category = e.Category,
                Term = e.Term,
                Definition = e.Definition,
                DefinitionEmbedding = e.DefinitionEmbedding,
            }
        );
        // 使用与步骤 1 中使用内存向量存储时相同的代码将数据摄入集合。
        await Step1_Ingest_Data.IngestDataIntoVectorStoreAsync(stringKeyCollection, ebdsvc);
        // 使用与步骤 2 中使用内存向量存储时相同的代码搜索向量存储。
        var searchResultItem = await Step2_Vector_Search.SearchVectorStoreAsync(
            stringKeyCollection,
            "什么是应用程序编程接口？",
            ebdsvc
        );
        // 将搜索结果及其得分输出到控制台。
        Console.WriteLine(searchResultItem.Record.Definition);
        Console.WriteLine(searchResultItem.Score);
    }

    /// <summary>
    /// 使用 ulong 作为键类型而非字符串的数据模型。
    /// </summary>
    private sealed class UlongGlossary
    {
        [VectorStoreRecordKey]
        public ulong Key { get; set; }

        [VectorStoreRecordData(IsFilterable = true)]
        public string Category { get; set; }

        [VectorStoreRecordData]
        public string Term { get; set; }

        [VectorStoreRecordData]
        public string Definition { get; set; }

        [VectorStoreRecordVector(Dimensions: 2560)]
        public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
    }

    /// <summary>
    /// 简单的装饰器类，允许将键和记录从一种类型转换为另一种类型。
    /// 实际上就是通过提供的key双向转换函数，以及整个记录的双向转换函数，在方法调用前后转换
    /// </summary>
    private sealed class MappingVectorStoreRecordCollection<
        TPublicKey,
        TInternalKey,
        TPublicRecord,
        TInternalRecord
    > : IVectorStoreRecordCollection<TPublicKey, TPublicRecord>
        where TPublicKey : notnull
        where TInternalKey : notnull
    {
        private readonly IVectorStoreRecordCollection<TInternalKey, TInternalRecord> _collection;
        private readonly Func<TPublicKey, TInternalKey> _publicToInternalKeyMapper;
        private readonly Func<TInternalKey, TPublicKey> _internalToPublicKeyMapper;
        private readonly Func<TPublicRecord, TInternalRecord> _publicToInternalRecordMapper;
        private readonly Func<TInternalRecord, TPublicRecord> _internalToPublicRecordMapper;
        public string CollectionName => this._collection.CollectionName;

        public MappingVectorStoreRecordCollection(
            IVectorStoreRecordCollection<TInternalKey, TInternalRecord> collection,
            Func<TPublicKey, TInternalKey> publicToInternalKeyMapper,
            Func<TInternalKey, TPublicKey> internalToPublicKeyMapper,
            Func<TPublicRecord, TInternalRecord> publicToInternalRecordMapper,
            Func<TInternalRecord, TPublicRecord> internalToPublicRecordMapper
        )
        {
            this._collection = collection;
            this._publicToInternalKeyMapper = publicToInternalKeyMapper;
            this._internalToPublicKeyMapper = internalToPublicKeyMapper;
            this._publicToInternalRecordMapper = publicToInternalRecordMapper;
            this._internalToPublicRecordMapper = internalToPublicRecordMapper;
        }

        public Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
        {
            return this._collection.CollectionExistsAsync(cancellationToken);
        }

        public Task CreateCollectionAsync(CancellationToken cancellationToken = default)
        {
            return this._collection.CreateCollectionAsync(cancellationToken);
        }

        public Task CreateCollectionIfNotExistsAsync(CancellationToken cancellationToken = default)
        {
            return this._collection.CreateCollectionIfNotExistsAsync(cancellationToken);
        }

        public Task DeleteAsync(
            TPublicKey key,
            DeleteRecordOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            return this._collection.DeleteAsync(
                this._publicToInternalKeyMapper(key),
                options,
                cancellationToken
            );
        }

        public Task DeleteBatchAsync(
            IEnumerable<TPublicKey> keys,
            DeleteRecordOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            return this._collection.DeleteBatchAsync(
                keys.Select(this._publicToInternalKeyMapper),
                options,
                cancellationToken
            );
        }

        public Task DeleteCollectionAsync(CancellationToken cancellationToken = default)
        {
            return this._collection.DeleteCollectionAsync(cancellationToken);
        }

        public async Task<TPublicRecord?> GetAsync(
            TPublicKey key,
            GetRecordOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var internalRecord = await this
                ._collection.GetAsync(
                    this._publicToInternalKeyMapper(key),
                    options,
                    cancellationToken
                )
                .ConfigureAwait(false);
            if (internalRecord == null)
            {
                return default;
            }

            return this._internalToPublicRecordMapper(internalRecord);
        }

        public IAsyncEnumerable<TPublicRecord> GetBatchAsync(
            IEnumerable<TPublicKey> keys,
            GetRecordOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var internalRecords = this._collection.GetBatchAsync(
                keys.Select(this._publicToInternalKeyMapper),
                options,
                cancellationToken
            );
            return internalRecords.Select(this._internalToPublicRecordMapper);
        }

        public async Task<TPublicKey> UpsertAsync(
            TPublicRecord record,
            UpsertRecordOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var internalRecord = this._publicToInternalRecordMapper(record);
            var internalKey = await this
                ._collection.UpsertAsync(internalRecord, options, cancellationToken)
                .ConfigureAwait(false);
            return this._internalToPublicKeyMapper(internalKey);
        }

        public async IAsyncEnumerable<TPublicKey> UpsertBatchAsync(
            IEnumerable<TPublicRecord> records,
            UpsertRecordOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            var internalRecords = records.Select(this._publicToInternalRecordMapper);
            var internalKeys = this._collection.UpsertBatchAsync(
                internalRecords,
                options,
                cancellationToken
            );
            await foreach (var internalKey in internalKeys.ConfigureAwait(false))
            {
                yield return this._internalToPublicKeyMapper(internalKey);
            }
        }

        public async Task<VectorSearchResults<TPublicRecord>> VectorizedSearchAsync<TVector>(
            TVector vector,
            VectorSearchOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var searchResults = await this
                ._collection.VectorizedSearchAsync(vector, options, cancellationToken)
                .ConfigureAwait(false);
            var publicResultRecords = searchResults.Results.Select(
                result => new VectorSearchResult<TPublicRecord>(
                    this._internalToPublicRecordMapper(result.Record),
                    result.Score
                )
            );

            return new VectorSearchResults<TPublicRecord>(publicResultRecords)
            {
                TotalCount = searchResults.TotalCount,
                Metadata = searchResults.Metadata,
            };
        }
    }
}
