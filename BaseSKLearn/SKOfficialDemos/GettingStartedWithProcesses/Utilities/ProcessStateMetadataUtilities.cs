using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process.Models;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Utilities;

/// <summary>
/// 该静态类提供了处理流程状态元数据的实用方法，包括将流程状态元数据保存到本地文件以及从本地文件加载流程状态元数据。
/// </summary>
public static class ProcessStateMetadataUtilities
{
    // 用于在仓库中存储 JSON 流程示例的路径
    private static readonly string s_currentSourceDir = Path.Combine(
        Directory.GetCurrentDirectory(),
        "..",
        "..",
        ".."
    );

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        // 格式化输出，使 JSON 更易读
        WriteIndented = true,
        // 忽略值为 null 的属性
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// 将流程状态元数据保存到本地文件
    /// </summary>
    /// <param name="processStateInfo">要保存的流程状态元数据</param>
    /// <param name="jsonFilename">JSON 文件名</param>
    public static void DumpProcessStateMetadataLocally(
        KernelProcessStateMetadata processStateInfo,
        string jsonFilename
    )
    {
        // 获取存储流程状态的文件路径
        var filepath = GetRepositoryProcessStateFilepath(jsonFilename);
        // 将流程状态保存到本地文件
        StoreProcessStateLocally(processStateInfo, filepath);
    }

    /// <summary>
    /// 从本地文件加载流程状态元数据
    /// </summary>
    /// <param name="jsonRelativePath">JSON 文件的相对路径</param>
    /// <returns>加载的流程状态元数据，如果文件不存在或解析失败则返回 null</returns>
    public static KernelProcessStateMetadata? LoadProcessStateMetadata(string jsonRelativePath)
    {
        // 获取存储流程状态的文件路径，并检查文件是否存在
        var filepath = GetRepositoryProcessStateFilepath(
            jsonRelativePath,
            checkFilepathExists: true
        );

        Console.WriteLine($"正在从以下路径加载流程状态元数据:\n'{Path.GetFullPath(filepath)}'");

        using StreamReader reader = new(filepath);
        var content = reader.ReadToEnd();
        // 反序列化 JSON 内容为流程状态元数据对象
        return JsonSerializer.Deserialize<KernelProcessStateMetadata>(content, s_jsonOptions);
    }

    /// <summary>
    /// 获取存储流程状态的文件路径
    /// </summary>
    /// <param name="jsonRelativePath">JSON 文件的相对路径</param>
    /// <param name="checkFilepathExists">是否检查文件路径是否存在，默认为 false</param>
    /// <returns>完整的文件路径</returns>
    /// <exception cref="KernelException">如果 checkFilepathExists 为 true 且文件路径不存在，抛出此异常</exception>
    private static string GetRepositoryProcessStateFilepath(
        string jsonRelativePath,
        bool checkFilepathExists = false
    )
    {
        string filepath = Path.Combine(s_currentSourceDir, jsonRelativePath);
        if (checkFilepathExists && !File.Exists(filepath))
        {
            throw new KernelException($"文件路径 {filepath} 不存在");
        }

        return filepath;
    }

    /// <summary>
    /// 将流程状态元数据以 JSON 格式存储到本地文件
    /// </summary>
    /// <param name="processStateInfo">要存储的流程状态元数据</param>
    /// <param name="fullFilepath">存储流程定义的完整文件路径，必须为 .json 扩展名</param>
    /// <exception cref="KernelException">如果文件所在目录不存在或文件扩展名不是 .json，抛出此异常</exception>
    private static void StoreProcessStateLocally(
        KernelProcessStateMetadata processStateInfo,
        string fullFilepath
    )
    {
        // 检查文件所在目录是否存在
        if (
            !(
                Path.GetDirectoryName(fullFilepath) is string directory
                && Directory.Exists(directory)
            )
        )
        {
            throw new KernelException(
                $"路径 '{fullFilepath}' 对应的目录不存在，无法保存流程 {processStateInfo.Name}"
            );
        }

        // 检查文件扩展名是否为 .json
        if (
            !(
                Path.GetExtension(fullFilepath) is string extension
                && !string.IsNullOrEmpty(extension)
                && extension == ".json"
            )
        )
        {
            throw new KernelException($"流程 {processStateInfo.Name} 的文件路径没有 .json 扩展名");
        }

        // 将流程状态元数据序列化为 JSON 字符串
        string content = JsonSerializer.Serialize(processStateInfo, s_jsonOptions);
        Console.WriteLine($"流程状态: \n{content}");
        Console.WriteLine($"正在将流程状态保存到本地: \n{Path.GetFullPath(fullFilepath)}");
        // 将 JSON 字符串写入文件
        File.WriteAllText(fullFilepath, content);
    }
}
