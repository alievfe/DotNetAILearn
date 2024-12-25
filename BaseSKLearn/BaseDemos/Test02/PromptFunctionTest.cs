using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SKUtils;

namespace BaseSKLearn;

[Experimental("SKEXP0010")]
public class PromptFunctionTest(Kernel kernel)
{
    public async Task String_Test()
    {
        var request =
            "I want to send an email to the marketing team celebrating their recent milestone.";
        var prompt = "What is the intent of this request? {{$request}}";

        var kf = kernel.CreateFunctionFromPrompt(prompt);

        // Create a kernel arguments object and add the request
        var ka = new KernelArguments { { "request", request } };

        var functionResult = await kf.InvokeAsync(kernel, ka);

        // SK还有更简单的方法可直接传prompts string 模版，方法内部实际上就是调用了CreateFunctionFromPrompt创建了kernel functions，目的是简化提示函数创建的过程
        // var functionResult = kernel.InvokePromptAsync(prompt, kernelArguments);

        Console.WriteLine(functionResult.ToString());
    }

    public async Task PromptTemplateConfig_Test()
    {
        string request =
            "I want to send an email to the marketing team celebrating their recent milestone.";

        var kernelFunction = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig
            {
                Name = "intent",
                Description = "use assistant to understand user input intent.",
                TemplateFormat = PromptTemplateConfig.SemanticKernelTemplateFormat, //此处可以省略默认就是"semantic-kernel"
                Template = "What is the intent of this request? {{$request}}",
                // 定义了输入变量request，包括的名字、描述和是否为必填项。这有助于明确哪些数据应该传递给Prompt，并且确保它们符合预期的格式
                InputVariables =
                [
                    new()
                    {
                        Name = "request",
                        Description = "The user's request.",
                        IsRequired = true,
                    },
                ],
                // OpenAI的执行设置，指定了最大令牌数（MaxTokens）为1024，以及温度（Temperature）为0。温度是用于控制生成文本随机性的参数，温度为0意味着生成的文本将是最可能的选项，没有随机性。
                ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
                {
                    {
                        OpenAIPromptExecutionSettings.DefaultServiceId, //"default"
                        new OpenAIPromptExecutionSettings { MaxTokens = 1024, Temperature = 0 }
                    },
                },
            }
        );
        var kernelArguments = new KernelArguments { { "request", request } };
        var functionResult = await kernelFunction.InvokeAsync(kernel, kernelArguments);

        Console.WriteLine(functionResult.ToString());
    }

    public async Task Yaml_Test()
    {
        //读取yaml文件地址
        var promptYaml = await File.ReadAllTextAsync(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test02", "joke.yaml")
        );
        KernelFunction jokeFunc = kernel.CreateFunctionFromPromptYaml(promptYaml);
        KernelArguments kernelArgs = new KernelArguments()
        {
            { "topic", "apple" },
            { "length", "3" },
        };
        // 用内核调用函数并提供kernelArguments
        FunctionResult results = await jokeFunc.InvokeAsync(kernel, kernelArgs);
        Console.WriteLine(results.ToString());
    }

    public async Task Handlebars_Test()
    {
        var template = """
            <message role="system">Instructions: What is the intent of this request?</message>
            <message role="user">{{request}}</message>
            """;
        var kernelFunction = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig()
            {
                Name = "getIntent",
                Description = "Understand the user's input intent.",
                TemplateFormat = HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat, // 获取 Handlebars 模板格式的名称。
                Template = template,
                InputVariables =
                [
                    new()
                    {
                        Name = "request",
                        Description = "User's request.",
                        IsRequired = true,
                    },
                ],
                ExecutionSettings = new Dictionary<string, PromptExecutionSettings>()
                {
                    {
                        OpenAIPromptExecutionSettings.DefaultServiceId,
                        new OpenAIPromptExecutionSettings() { MaxTokens = 2048, Temperature = 0.6 }
                    },
                },
            },
            new HandlebarsPromptTemplateFactory()
        );

        string request =
            "I want to send an email to the marketing team celebrating their recent milestone.";
        var result = await kernelFunction.InvokeAsync(
            kernel,
            new KernelArguments() { { "request", request } }
        );
        Console.WriteLine(result.ToString());
    }
}
