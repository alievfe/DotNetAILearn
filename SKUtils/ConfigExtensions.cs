﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SKUtils;

public static class ConfigExtensions
{
    public static T GetConfig<T, P>(string sectionName)
        where P : class =>
        LoadConfigFromSecrets<P>().GetSection(sectionName).Get<T>()
        ?? throw new InvalidDataException("Invalid semantic kernel configuration is empty");

    public static T GetConfig<T>(string jsonPath, string sectionName) =>
        LoadConfigFromJson(jsonPath).GetSection(sectionName).Get<T>()
        ?? throw new InvalidDataException("Invalid semantic kernel configuration is empty");

    public static IConfigurationRoot LoadConfigFromSecrets<P>()
        where P : class => new ConfigurationBuilder().AddUserSecrets<P>().Build();

    public static IConfigurationRoot LoadConfigFromJson(string jsonPath = "./tmpsecrets.json") =>
        new ConfigurationBuilder().AddJsonFile(jsonPath).Build();

    public static IKernelBuilder AddOpenAIChat(this IKernelBuilder builder, OpenAIConfig config)
    {
        return builder.AddOpenAIChatCompletion(
            modelId: config.ModelId,
            apiKey: config.ApiKey,
            endpoint: config.Endpoint
        );
    }

    public static IKernelBuilder AddOpenAIChatWithHttpClient(
        this IKernelBuilder builder,
        OpenAIConfig config,
        bool isLog = false
    )
    {
        return builder.AddOpenAIChatCompletion(
            modelId: config.ModelId,
            apiKey: config.ApiKey,
            httpClient: new HttpClient(new AIHostCustomHandler(config.Host, isLog))
        );
    }

    public static IKernelBuilder AddOpenAIEmbedding(
        this IKernelBuilder builder,
        OpenAIConfig config
    )
    {
        return builder.AddOpenAITextEmbeddingGeneration(
            modelId: config.ModelId,
            apiKey: config.ApiKey,
            httpClient: new HttpClient(new AIHostCustomHandler(config.Host))
        );
    }

    public static Kernel GetKernel<P>(string chatModelName, string? ebdModelName = null)
        where P : class
    {
        var configRoot = LoadConfigFromSecrets<P>();
        var chatConfig = configRoot.GetSection(chatModelName).Get<OpenAIConfig>();
        var ebdConfig = configRoot.GetSection(ebdModelName).Get<OpenAIConfig>();
        var builder = Kernel.CreateBuilder().AddOpenAIChat(chatConfig);
        if (ebdModelName != null)
            builder.AddOpenAIEmbedding(ebdConfig);
        return builder.Build();
    }

    public static Kernel GetKernel(
        string chatModelName,
        string? ebdModelName = null,
        string jsonPath = "./tmpsecrets.json"
    )
    {
        var configRoot = LoadConfigFromJson(jsonPath);
        var chatConfig = configRoot.GetSection(chatModelName).Get<OpenAIConfig>();
        var builder = Kernel.CreateBuilder().AddOpenAIChat(chatConfig);
        if (ebdModelName != null)
        {
            var ebdConfig = configRoot.GetSection(ebdModelName).Get<OpenAIConfig>();
            builder.AddOpenAIEmbedding(ebdConfig);
        }
        return builder.Build();
    }

    public static Kernel GetKernel2(
        string chatModelName,
        string? ebdModelName = null,
        string jsonPath = "./tmpsecrets.json",
        bool isLog = true
    )
    {
        var configRoot = LoadConfigFromJson(jsonPath);
        var chatConfig = configRoot.GetSection(chatModelName).Get<OpenAIConfig>();
        var builder = Kernel.CreateBuilder().AddOpenAIChatWithHttpClient(chatConfig, isLog);
        if (ebdModelName != null)
        {
            var ebdConfig = configRoot.GetSection(ebdModelName).Get<OpenAIConfig>();
            builder.AddOpenAIEmbedding(ebdConfig);
        }
        return builder.Build();
    }

    public static Kernel GetKernelEmbedding(string jsonPath, string ebdModelName)
    {
        var config = GetConfig<OpenAIConfig>(jsonPath, ebdModelName);
        var builder = Kernel.CreateBuilder().AddOpenAIEmbedding(config);
        return builder.Build();
    }

    public static OpenAITextEmbeddingGenerationService GetEbdService(
        string ebdModelName = "DouBao-Ebd"
    )
    {
        var ebdConfig = LoadConfigFromJson().GetSection(ebdModelName).Get<OpenAIConfig>();
        return new OpenAITextEmbeddingGenerationService(
            modelId: ebdConfig.ModelId,
            apiKey: ebdConfig.ApiKey,
            httpClient: new HttpClient(new AIHostCustomHandler(ebdConfig.Host))
        );
    }

    public static WeatherAPI GetWeatherAPI(string jsonPath) =>
        new(GetConfig<string>(jsonPath, "WeatherApiKey"));
}
