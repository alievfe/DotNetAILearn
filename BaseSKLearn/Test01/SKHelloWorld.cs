using System.Diagnostics.CodeAnalysis;
using BaseSKLearn.Utils;
using Microsoft.SemanticKernel;

namespace BaseSKLearn;

public class SKHelloWorld
{
    [Experimental("SKEXP0010")]
    public static async Task Test()
    {
        var config = ConfigExtensions.FromSecretsConfig<OpenAIConfig>("OneApiSpark");
        // using HttpClient httpClient = new(new OneAPICustomHandler(config.Host));

        // create Kernel
        var kernel = Kernel
            .CreateBuilder()
            // .AddOpenAIChatCompletion(
            //     modelId: config.ModelId,
            //     apiKey: config.ApiKey,
            //     httpClient: httpClient
            // )
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();

        // 用户输入
        var request =
            "I want to send an email to the marketing team celebrating their recent milestone";

        // create prompt
        var prompt = "这个请求的意图是什么? {{$request}}";

        // Create a kernel arguments object and add the request
        var ka = new KernelArguments { { nameof(request), request } };

        // output
        await foreach (var streamingKernelContent in kernel.InvokePromptStreamingAsync(prompt, ka))
        {
            Console.WriteLine(streamingKernelContent);
        }

        // var res = await kernel.InvokePromptAsync(prompt,ka);
        // Console.WriteLine(res);
    }
}
