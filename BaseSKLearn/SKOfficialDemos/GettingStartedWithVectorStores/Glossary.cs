using Microsoft.Extensions.VectorData;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithVectorStores;

/// <summary>
/// 表示词汇表条目的示例模型类。
/// </summary>
/// <remarks>
/// 请注意，每个属性都使用一个特性进行修饰，该特性指定了向量存储应如何处理该属性。
/// 从而能够在向量存储中创建一个集合，并在无需任何进一步配置的情况下插入或更新以及检索此类的实例。
/// </remarks>
internal sealed class Glossary
{
    /// <summary>
    /// 词汇表条目的唯一键值。
    /// </summary>
    /// <remarks>
    /// 在向量存储中用于唯一标识一个记录。
    /// </remarks>
    [VectorStoreRecordKey]
    public string Key { get; set; }

    /// <summary>
    /// 词汇表条目所属的类别。
    /// </summary>
    /// <remarks>
    /// IsFilterable属性为true，表示可以在查询时根据此属性过滤记录。
    /// </remarks>
    [VectorStoreRecordData(IsFilterable = true)]
    public string Category { get; set; }

    /// <summary>
    /// 词汇表中的术语。
    /// </summary>
    [VectorStoreRecordData]
    public string Term { get; set; }

    /// <summary>
    /// 与术语相关的定义。
    /// </summary>
    [VectorStoreRecordData]
    public string Definition { get; set; }

    /// <summary>
    /// 定义的向量嵌入。
    /// </summary>
    /// <remarks>
    /// VectorStoreRecordVectorAttribute指定了维度大小为2560。
    /// 用于存储定义文本的向量表示形式，以便支持相似性搜索等高级功能。
    /// </remarks>
    [VectorStoreRecordVector(Dimensions: 2560)]
    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}