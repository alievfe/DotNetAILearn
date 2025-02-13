using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace SKUtils.Web;

internal class ShaBingSearch : ITextSearch
{
    public async Task<KernelSearchResults<object>> GetSearchResultsAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        //searchOptions ??= new TextSearchOptions();
        //BingSearchResponse<BingWebPage>? searchResponse = await this.ExecuteSearchAsync(query, searchOptions, cancellationToken).ConfigureAwait(false);

        //long? totalCount = searchOptions.IncludeTotalCount ? searchResponse?.WebPages?.TotalEstimatedMatches : null;

        //return new KernelSearchResults<string>(this.GetResultsAsStringAsync(searchResponse, cancellationToken), totalCount, GetResultsMetadata(searchResponse));
        throw new NotImplementedException();
    }

    public Task<KernelSearchResults<TextSearchResult>> GetTextSearchResultsAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<KernelSearchResults<string>> SearchAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
