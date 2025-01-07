using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Resources;
using SKUtils;

namespace BaseSKLearn.SKOfficialDemos.GettingStarted;

/// <summary>
/// 此示例展示了如何从 YAML 资源创建一个提示 <see cref="KernelFunction"/>。
/// </summary>
public sealed class Step3_Yaml_Prompt
{
    /// <summary>
    /// 展示如何从 YAML 资源创建一个提示 <see cref="KernelFunction"/>。
    /// </summary>
    public async Task CreatePromptFromYamlAsync()
    {
        // 使用 OpenAI 的聊天完成功能创建内核
        Kernel kernel = ConfigExtensions.GetKernel("DouBao");

        // 从资源加载提示
        var generateStoryYaml = EmbeddedResource.Read("GenerateStory.yaml");
        var function = kernel.CreateFunctionFromPromptYaml(generateStoryYaml);

        // 调用提示函数并显示结果
        Console.WriteLine(
            await kernel.InvokeAsync(
                function,
                arguments: new() { { "topic", "Dog" }, { "length", "3" } }
            )
        );

        // 从资源加载 Handlebars 模板的提示
        var generateStoryHandlebarsYaml = EmbeddedResource.Read("GenerateStoryHandlebars.yaml");
        function = kernel.CreateFunctionFromPromptYaml(
            generateStoryHandlebarsYaml,
            new HandlebarsPromptTemplateFactory()
        );

        // 调用提示函数并显示结果
        Console.WriteLine(
            await kernel.InvokeAsync(
                function,
                arguments: new() { { "topic", "Cat" }, { "length", "3" } }
            )
        );
    }
}
