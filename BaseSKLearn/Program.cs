using System.Text.Json;
using BaseSKLearn;
using Microsoft.SemanticKernel;
using SKUtils;

#pragma warning disable SKEXP0010
Console.WriteLine("Hello, World!");

var kernel = ConfigExtensions.GetKernel("./tmpsecrets.json", "InternLM");

// await new SKHelloWorld(kernel).Test();
// await new FunctionCallingTest(
//     kernel,
//     ConfigExtensions.GetWeatherAPI("./tmpsecrets.json")
// ).ManuallyCall_Test();

Console.ReadLine();
