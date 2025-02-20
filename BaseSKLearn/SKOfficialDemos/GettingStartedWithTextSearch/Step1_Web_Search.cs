using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using SKUtils.TestUtils;
using SKUtils.Web;
using Xunit.Abstractions;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithTextSearch;

/// <summary>
/// 此示例展示了如何创建和使用 <see cref="ITextSearch"/>。
/// </summary>
public class Step1_Web_Search
{
    /// <summary>
    /// 展示如何创建 <see cref="BingTextSearch"/> 并使用它进行搜索。
    /// </summary>
    public async Task ShaBingSearchAsync()
    {
        // 使用必应搜索创建一个 ITextSearch 实例
        var textSearch = new ShaBingSearch();

        var query = "我又幻想了是什么梗？";

        // 进行搜索并返回结果
        KernelSearchResults<string> searchResults = await textSearch.SearchAsync(
            query,
            new() { Top = 4 }
        );
        await foreach (string result in searchResults.Results)
        {
            Console.WriteLine(result);
        }
    }

    /// <summary>
    /// 展示如何创建 <see cref="ShaBingSearch"/> 并使用它进行搜索，
    /// 并将结果作为 <see cref="ShaBingWebPage"/> 实例的集合返回。
    /// </summary>
    public async Task SearchForWebPagesAsync()
    {
        // 创建一个 ITextSearch 实例
        var textSearch = new ShaBingSearch();

        var query = "什么是语义内核？";

        // 进行搜索并使用特定于实现的数据模型返回结果
        KernelSearchResults<object> objectResults = await textSearch.GetSearchResultsAsync(
            query,
            new() { Top = 4 }
        );
        Console.WriteLine("\n--- 必应网页搜索结果 ---\n");
        await foreach (ShaBingWebPage webPage in objectResults.Results)
        {
            Console.WriteLine($"名称:            {webPage.Name}");
            Console.WriteLine($"摘要:         {webPage.Snippet}");
            Console.WriteLine($"网址:             {webPage.Url}");
            Console.WriteLine($"显示网址:      {webPage.DisplayUrl}");
            Console.WriteLine($"最后爬取日期: {webPage.DateLastCrawled}");
        }
    }

    /// <summary>
    /// 展示如何创建 <see cref="BingTextSearch"/> 并使用它进行搜索，
    /// 并将结果作为 <see cref="TextSearchResult"/> 实例的集合返回。
    /// </summary>
    /// <remarks>
    /// 当你希望以一致的方式处理不同搜索服务的结果时，使用标准化的搜索结果格式会很有用。
    /// </remarks>
    public async Task SearchForTextSearchResultsAsync()
    {
        // 创建一个 ITextSearch 实例
        var textSearch = new ShaBingSearch();


        var query = "什么是语义内核？";

        // 进行搜索并将结果作为 TextSearchResult 项返回
        KernelSearchResults<TextSearchResult> textResults =
            await textSearch.GetTextSearchResultsAsync(query, new() { Top = 4 });
        Console.WriteLine("\n--- 文本搜索结果 ---\n");
        await foreach (TextSearchResult result in textResults.Results)
        {
            Console.WriteLine($"名称:  {result.Name}");
            Console.WriteLine($"内容: {result.Value}");
            Console.WriteLine($"链接:  {result.Link}");
        }
    }
}
