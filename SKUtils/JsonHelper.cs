using System;
using System.Text.Json;

namespace SKUtils;

public static class JsonHelper
{
    /// <summary>
    /// 序列化给定的对象并将其保存为当前目录下的output.json文件。
    /// </summary>
    /// <typeparam name="T">要序列化的对象类型。</typeparam>
    /// <param name="obj">要序列化的对象实例。</param>
    /// <param name="fileName">输出文件名，默认为"output.json"。</param>
    public static async Task SerializeObjectToFile<T>(this T obj, string fileName = "./output.json")
        where T : class
    {
        try
        {
            // 序列化对象为JSON字符串
            string jsonString =
                JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true })
                + Environment.NewLine;

            // 获取当前执行程序的目录路径
            string currentDirectory = Directory.GetCurrentDirectory();

            // 定义输出文件的完整路径
            string outputPath = Path.Combine(currentDirectory, fileName);

            // 将JSON字符串写入文件
            await File.AppendAllTextAsync(outputPath, jsonString);

            Console.WriteLine($"The object has been serialized and saved to: {outputPath}");
        }
        catch (Exception ex)
        {
            // 捕获异常并打印错误信息
            Console.WriteLine($"An error occurred while serializing the object: {ex.Message}");
            throw; // 重新抛出异常以供调用者处理
        }
    }
}
