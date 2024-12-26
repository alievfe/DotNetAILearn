using System;
using Microsoft.Extensions.VectorData;

namespace BaseSKLearn.Models;

public class Glossary
{
    [VectorStoreRecordKey]
    public ulong Key {get; set;}

    [VectorStoreRecordData]
    public string Term { get; set; }

    [VectorStoreRecordData]
    public string Definition { get; set; }

    [VectorStoreRecordVector(Dimensions: 2560)]
    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}
