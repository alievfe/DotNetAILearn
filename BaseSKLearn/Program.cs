using System.Text.Json;
using BaseSKLearn;
using BaseSKLearn.Plugins.MathPlg;
using BaseSKLearn.XZYDemos;
using Microsoft.SemanticKernel;
using SKUtils;

Console.WriteLine("Hello, World!");

// var kernel = ConfigExtensions.GetKernel("./tmpsecrets.json", "InternLM");
var kernel = ConfigExtensions.GetKernel2("./tmpsecrets.json", "DouBao");
// await new SKHelloWorld(kernel).Test();
// await new FunctionCallingTest(
//     kernel,
//     ConfigExtensions.GetWeatherAPI("./tmpsecrets.json")
// ).AutoCall_Test();

// await new SKXZYTest(kernel).Translate("你好","EN");
// await new SKXZYTest(kernel).PlanTest("小明有7个冰淇淋，我有2个冰淇淋，他比我多几个冰淇淋？");
// await new SKXZYTest(kernel).PipelineTest();
await new SKXZYTest(kernel).TextChunkTest();

// await new VectorStoresAndEmbeddingsTest(kernel).InMemoryEmbeddingTest();
// await SKMemoryXZYTest.DI();

Console.ReadLine();
