using System.Text.Json;
using BaseSKLearn;
using BaseSKLearn.Plugins.MathPlg;
using Microsoft.SemanticKernel;
using SKUtils;

#pragma warning disable SKEXP0010
Console.WriteLine("Hello, World!");

var kernel = ConfigExtensions.GetKernel("./tmpsecrets.json", "Qwen");

// await new SKHelloWorld(kernel).Test();
// await new FunctionCallingTest(
//     kernel,
//     ConfigExtensions.GetWeatherAPI("./tmpsecrets.json")
// ).ManuallyCall_Test();

// await new SKXZYTest(kernel).Translate("你好","EN");
await new SKXZYTest(kernel).NativeNestedFunc("13,1,2,3");

Console.ReadLine();
