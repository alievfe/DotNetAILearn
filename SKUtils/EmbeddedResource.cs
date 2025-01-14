using System;
using System.Reflection;
using SKUtils;

namespace Resources;

/// <summary>
/// 用于加载嵌入程序集中的资源的资源辅助类。默认情况下，我们只嵌入文本文件，
/// 因此该辅助类仅限于返回文本。
///
/// 您可以在这里找到有关嵌入式资源的信息：
/// * https://learn.microsoft.com/dotnet/core/extensions/create-resource-files
/// * https://learn.microsoft.com/dotnet/api/system.reflection.assembly.getmanifestresourcestream?view=net-7.0
///
/// 要了解哪些资源已嵌入，请检查 csproj 文件。
/// </summary>
public static class EmbeddedResource
{
    private static readonly string? s_namespace = typeof(EmbeddedResource).Namespace;

    public static string Read(string fileName)
    {
        // 获取当前程序集。注意：此类位于存储嵌入资源的同一个程序集中。
        Assembly assembly =
            typeof(EmbeddedResource).GetTypeInfo().Assembly
            ?? throw new ConfigurationNotFoundException($"[{s_namespace}] {fileName} 程序集未找到");

        // 资源映射类似于类型，使用命名空间并追加 "."（点）和文件名
        var resourceName = $"{s_namespace}." + fileName;
        using Stream resource =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new ConfigurationNotFoundException($"{resourceName} 资源未找到");

        // 返回文本格式的资源内容。
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    public static Stream? ReadStream(string fileName)
    {
        // 获取当前程序集。注意：此类位于存储嵌入资源的同一个程序集中。
        Assembly assembly =
            typeof(EmbeddedResource).GetTypeInfo().Assembly
            ?? throw new ConfigurationNotFoundException($"[{s_namespace}] {fileName} 程序集未找到");

        // 资源映射类似于类型，使用命名空间并追加 "."（点）和文件名
        var resourceName = $"{s_namespace}." + fileName;
        return assembly.GetManifestResourceStream(resourceName);
    }

    public static async Task<ReadOnlyMemory<byte>> ReadAllAsync(string fileName)
    {
        await using Stream? resourceStream = ReadStream(fileName);
        using var memoryStream = new MemoryStream();

        // 将资源流复制到内存流
        await resourceStream!.CopyToAsync(memoryStream);

        // 将内存流的缓冲区转换为 ReadOnlyMemory<byte>
        // 注意：ToArray() 创建了缓冲区的副本，这对于转换为 ReadOnlyMemory<byte> 是可以接受的
        return new ReadOnlyMemory<byte>(memoryStream.ToArray());
    }
}
