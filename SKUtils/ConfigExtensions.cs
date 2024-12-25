using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace SKUtils;

public static class ConfigExtensions
{
    public static T FromSecretsConfig<T, P>(string sectionName)
        where P : class =>
        new ConfigurationBuilder().AddUserSecrets<P>().Build().GetSection(sectionName).Get<T>()
        ?? throw new InvalidDataException("Invalid semantic kernel configuration is empty");

    public static T FromJsonConfig<T>(string jsonPath, string sectionName) =>
        new ConfigurationBuilder().AddJsonFile(jsonPath).Build().GetSection(sectionName).Get<T>()
        ?? throw new InvalidDataException("Invalid semantic kernel configuration is empty");

    [Experimental("SKEXP0010")]
    public static Kernel GetKernel<P>(string llmName)
        where P : class
    {
        var config = FromSecretsConfig<OpenAIConfig, P>(llmName);
        return Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();
    }

    [Experimental("SKEXP0010")]
    public static Kernel GetKernel(string jsonPath, string llmName)
    {
        var config = FromJsonConfig<OpenAIConfig>(jsonPath, llmName);
        return Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: config.Endpoint
            )
            .Build();
    }

    public static WeatherAPI GetWeatherAPI(string jsonPath) =>
        new(FromJsonConfig<string>(jsonPath, "WeatherApiKey"));
}
