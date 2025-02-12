using Microsoft.SemanticKernel.Data;

namespace SKUtils.Web;

internal class UnOfficialBingSearch : ITextSearch
{
    public Task<KernelSearchResults<object>> GetSearchResultsAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
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
