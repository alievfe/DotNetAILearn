using System;
using System.Reflection;
using PuppeteerSharp;

namespace SKUtils;

/// <summary>
/// 使用 Puppeteer-Sharp 将 Mermaid 图表渲染为图像。
/// </summary>
public class MermaidRenderer
{
    /// <summary>
    /// 从提供的 Mermaid 代码生成 Mermaid 图表图像。
    /// </summary>
    /// <param name="mermaidCode">Mermaid 代码。</param>
    /// <param name="filenameOrPath">输出文件的名称或路径。</param>
    /// <returns>返回生成的图像文件路径。</returns>
    /// <exception cref="InvalidOperationException">当发生意外错误时抛出。</exception>
    public static async Task<string> GenerateMermaidImageAsync(
        string mermaidCode,
        string filenameOrPath
    )
    {
        // 确保文件名具有正确的 .png 扩展名
        if (!filenameOrPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("文件名必须具有 .png 扩展名。", nameof(filenameOrPath));
        }
        string outputFilePath;
        // 检查用户是否提供了绝对路径
        if (Path.IsPathRooted(filenameOrPath))
        {
            // 使用提供的绝对路径
            outputFilePath = filenameOrPath;

            // 确保目录存在
            string directoryPath =
                Path.GetDirectoryName(outputFilePath)
                ?? throw new InvalidOperationException("无法确定目录路径。");
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"目录 '{directoryPath}' 不存在。");
            }
        }
        else
        {
            // 对于相对路径，使用程序集的目录
            string? assemblyPath =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? throw new InvalidOperationException("无法确定程序集路径。");
            string outputPath = Path.Combine(assemblyPath, "output");
            Directory.CreateDirectory(outputPath); // 确保输出目录存在
            outputFilePath = Path.Combine(outputPath, filenameOrPath);
        }
        // 如果尚未安装 Chromium，则下载
        BrowserFetcher browserFetcher = new();
        browserFetcher.Browser = SupportedBrowser.Chrome;
        await browserFetcher.DownloadAsync();

        // 定义包含 Mermaid.js CDN 的 HTML 模板
        string htmlContent =
            $@"
        <html>
            <head>
                <style>
                    body {{
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        margin: 0;
                        height: 100vh;
                    }}
                </style>
                <script type=""module"">
                    import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
                    mermaid.initialize({{ startOnLoad: true }});
                </script>
            </head>
            <body>
                <div class=""mermaid"">
                    {mermaidCode}
                </div>
            </body>
        </html>";

        // 创建一个包含 Mermaid 代码的临时 HTML 文件
        string tempHtmlFile = Path.Combine(Path.GetTempPath(), "mermaid_temp.html");
        try
        {
            await File.WriteAllTextAsync(tempHtmlFile, htmlContent);

            // 使用 Puppeteer-Sharp 启动无头浏览器以渲染 Mermaid 图表
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            await page.GoToAsync($"file://{tempHtmlFile}");
            await page.WaitForSelectorAsync(".mermaid"); // 等待 Mermaid 渲染完成
            await page.ScreenshotAsync(
                outputFilePath,
                new ScreenshotOptions { FullPage = true }
            );
        }
        catch (IOException ex)
        {
            throw new IOException("访问文件时发生错误。", ex);
        }
        catch (Exception ex) // 捕获可能发生的其他异常
        {
            throw new InvalidOperationException("在渲染 Mermaid 图表时发生意外错误。", ex);
        }
        finally
        {
            // 清理临时 HTML 文件
            if (File.Exists(tempHtmlFile))
            {
                File.Delete(tempHtmlFile);
            }
        }

        return outputFilePath;
    }
}
