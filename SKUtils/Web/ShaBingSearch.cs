using Microsoft.Playwright;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace SKUtils.Web;

/// <summary>
/// 可用于使用 Unofficial Bing Web Search API 执行搜索的 Sha Bing 文本搜索实现。
/// </summary>
public class ShaBingSearch : ITextSearch
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private string _host = "https://cn.bing.com";

    public ShaBingSearch(string? host = null)
    {
        _host = host ?? _host;

        _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        _browser = _playwright
            .Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true })
            .GetAwaiter()
            .GetResult();
    }

    public async Task<KernelSearchResults<object>> GetSearchResultsAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default
    )
    {
        searchOptions ??= new TextSearchOptions();
        ShaBingSearchResponse<ShaBingWebPage>? searchResponse = await this.ExecuteSearchAsync(
                query,
                searchOptions,
                cancellationToken
            )
            .ConfigureAwait(false);

        //long? totalCount = searchOptions.IncludeTotalCount ? searchResponse?.WebPages?.TotalEstimatedMatches : null;

        //return new KernelSearchResults<string>(this.GetResultsAsStringAsync(searchResponse, cancellationToken), totalCount, GetResultsMetadata(searchResponse));
        throw new NotImplementedException();
    }

    public Task<KernelSearchResults<TextSearchResult>> GetTextSearchResultsAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<KernelSearchResults<string>> SearchAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    #region private

    /// <summary>
    /// 执行 Sha Bing 搜索查询并返回结果。
    /// </summary>
    /// <param name="query">要搜索的内容。</param>
    /// <param name="searchOptions">搜索选项。</param>
    /// <param name="cancellationToken">用于监视取消请求的 <see cref="CancellationToken"/>。默认值为 <see cref="CancellationToken.None"/>。</param>
    private async Task<ShaBingSearchResponse<ShaBingWebPage>?> ExecuteSearchAsync(
        string query,
        TextSearchOptions searchOptions,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    #endregion
}
