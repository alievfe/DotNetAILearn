using System.ComponentModel;
using System.Text.Json;
using BaseSKLearn;
using BaseSKLearn.Plugins.MathPlg;
using BaseSKLearn.SKOfficialDemos.GettingStarted;
using BaseSKLearn.SKOfficialDemos.GettingStartedWithAgents;
using BaseSKLearn.XZYDemos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using SKUtils;

Console.WriteLine("Hello, World!");

// var kernel = ConfigExtensions.GetKernel("./tmpsecrets.json", "InternLM");
// var kernel = ConfigExtensions.GetKernel2("./tmpsecrets.json", "DouBao");
// await new SKHelloWorld(kernel).Test();
// await new FunctionCallingTest(
//     kernel,
//     ConfigExtensions.GetWeatherAPI("./tmpsecrets.json")
// ).AutoCall_Test();

// await new SKXZYTest(kernel).Translate("你好","EN");
// await new SKXZYTest(kernel).PlanTest("小明有7个冰淇淋，我有2个冰淇淋，他比我多几个冰淇淋？");
// await new SKXZYTest(kernel).PipelineTest();
// await new SKXZYTest(kernel).TextChunkTest();

// await new VectorStoresAndEmbeddingsTest(kernel).InMemoryEmbeddingTest();
// await SKMemoryXZYTest.DI();

// await new Step1_Create_Kernel().CreateKernelAsync();
// await new Step2_Add_Plugins().AddPluginsAsync();
// await new Step3_Yaml_Prompt().CreatePromptFromYamlAsync();
// await new Step6_Responsible_AI().AddPromptFilterAsync();
// await new Step7_Observability().ObservabilityWithFiltersAsync();
// await new Step8_Pipelining().CreateFunctionPipelineAsync();
// await new Step9_OpenAPI_Plugins().AddOpenAPIPluginsAsync();
// await new Step9_OpenAPI_Plugins().TransformOpenAPIPluginsAsync();
// await new Step01_Agent().UseSingleChatComplectionAgentAsync();
// await new Step01_Agent().UseTemplateForChatCompletionAgentAsync();
// await new Step02_Plugins().UseChatCompletionWithPluginAgentAsync();
// await new Step03_Chat().UseAgentGroupChatWithTwoAgentsAsync();
// await new Step03_Chat().UseAgentGroupChatWithTwoAgentsForCNAsync();
// await new Step04_KernelFunctionStrategies().UseKernelFunctionStrategiesWithAgentGroupChatAsync();
// await new Step05_JsonResult().UseKernelFunctionStrategiesWithJsonResultAsync();
// await new Step06_DependencyInjection().UseDependencyInjectionToCreateAgentAsync();
// await new Step08_Assistant().UseChatCompletionWithPluginAgentAsync();




















Console.ReadLine();

internal class Worker(
    IHostApplicationLifetime hostLifetime,
    Kernel kernel
) : IHostedService
{
    private int? _exitCode;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            //
            _exitCode = 0;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
            _exitCode = 1;
        }
        finally
        {
            hostLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
        return Task.CompletedTask;
    }
}

public class DITest
{
    public static async Task DI()
    {
        await Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder => { })
            .ConfigureServices(
                (hostContext, services) =>
                {
                    var chatConfig = ConfigExtensions.GetConfig<OpenAIConfig>(
                        "./tmpsecrets.json",
                        "DouBao"
                    );
                    var ebdConfig = ConfigExtensions.GetConfig<OpenAIConfig>(
                        "./tmpsecrets.json",
                        "DouBao-Ebd"
                    );

                    // 注册kernel
                    services
                        .AddKernel()
                        .AddOpenAIChatCompletion(
                            modelId: chatConfig.ModelId,
                            apiKey: chatConfig.ApiKey,
                            endpoint: chatConfig.Endpoint
                        )
                        .AddOpenAITextEmbeddingGeneration(
                            modelId: ebdConfig.ModelId,
                            apiKey: ebdConfig.ApiKey,
                            httpClient: new HttpClient(new AIHostCustomHandler(ebdConfig.Host))
                        );

                    services.AddSqliteVectorStore("Data Source=E:/Develop/SqliteData/MyApp.db");

                    services.AddHostedService<Worker>();
                }
            )
            .RunConsoleAsync();
    }
}
//
