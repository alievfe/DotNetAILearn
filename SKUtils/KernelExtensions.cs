using System;

namespace Microsoft.SemanticKernel;

public static class KernelExtensions
{
    public static KernelPlugin ImportPluginFromDefaultPathPromptDirectory(
        this Kernel kernel,
        string pluginDirName,
        string? pluginName = null,
        IPromptTemplateFactory? promptTemplateFactory = null
    ) =>
        kernel.ImportPluginFromPromptDirectory(
            Path.Combine(Directory.GetCurrentDirectory(), "Plugins", pluginDirName),
            pluginName,
            promptTemplateFactory
        );
}
