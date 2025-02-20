using Microsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using SKUtils;
using SKUtils.Web;
using System.Linq;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithTextSearch;

public class Step2_Search_For_RAG
{
    /// <summary>
    /// 展示如何从 <see cref="ShaBingSearch"/> 创建默认的 <see cref="KernelPlugin"/>，并使用它为提示添加背景上下文。
    /// </summary>
    [Fact]
    public async Task RagWithTextSearchAsync()
    {
        Kernel kernel = ConfigExtensions.GetKernel2("DouBao");

        // 使用 Bing 搜索创建文本搜索实例
        var textSearch = new ShaBingSearch();

        // 构建一个带有网页搜索功能的文本搜索插件，并添加到内核中
        var searchPlugin = CreateWithSearch(textSearch, "SearchPlugin");
        kernel.Plugins.Add(searchPlugin);

        // 调用提示，并使用文本搜索插件提供背景信息
        var query = "我又幻想了是什么梗？";
        var prompt = "{{$query}}  搜索结果: {{SearchPlugin.Search $query}}. ";
        KernelArguments arguments = new() { { "query", query } };
        Console.WriteLine(await kernel.InvokePromptAsync(prompt, arguments));
    }


    private static KernelPlugin CreateWithSearch(ITextSearch textSearch, string pluginName, string? description = null)
    {
        return KernelPluginFactory.CreateFromFunctions(pluginName, description, [CreateSearch(textSearch)]);
    }

    private static KernelFunction CreateSearch(ITextSearch textSearch, KernelFunctionFromMethodOptions? options = null, TextSearchOptions? searchOptions = null)
    {
        async Task<string> SearchAsync(Kernel kernel, KernelFunction function, KernelArguments arguments, CancellationToken cancellationToken, int count = 3, int skip = 0)
        {
            arguments.TryGetValue("query", out var query);
            if (string.IsNullOrEmpty(query?.ToString()))
            {
                return "";
            }

            var parameters = function.Metadata.Parameters;

            searchOptions ??= new()
            {
                Top = count,
                Skip = skip,
                Filter = CreateBasicFilter(options, arguments)
            };

            var result = await textSearch.SearchAsync(query?.ToString()!, searchOptions, cancellationToken).ConfigureAwait(false);
            var resultList = await result.Results.ToListAsync(cancellationToken).ConfigureAwait(false);
            return string.Join("\n\n", resultList);
        }

        options ??= DefaultSearchMethodOptions();
        return KernelFunctionFactory.CreateFromMethod(
                SearchAsync,
                options);
    }

    private static TextSearchFilter? CreateBasicFilter(KernelFunctionFromMethodOptions? options, KernelArguments arguments)
    {
        if (options?.Parameters is null)
        {
            return null;
        }

        TextSearchFilter? filter = null;
        foreach (var parameter in options.Parameters)
        {
            // treat non standard parameters as equality filter clauses
            if (!parameter.Name.Equals("query", System.StringComparison.Ordinal) &&
                !parameter.Name.Equals("count", System.StringComparison.Ordinal) &&
                !parameter.Name.Equals("skip", System.StringComparison.Ordinal))
            {
                if (arguments.TryGetValue(parameter.Name, out var value) && value is not null)
                {
                    filter ??= new TextSearchFilter();
                    filter.Equality(parameter.Name, value);
                }
            }
        }

        return filter;
    }

    private static KernelFunctionFromMethodOptions DefaultSearchMethodOptions() =>
    new()
    {
        FunctionName = "Search",
        Description = "Perform a search for content related to the specified query and return string results",
        Parameters = GetDefaultKernelParameterMetadata(),
        ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) },
    };

    private static IEnumerable<KernelParameterMetadata> GetDefaultKernelParameterMetadata()
    {
        return  [
            new KernelParameterMetadata("query") { Description = "What to search for", ParameterType = typeof(string), IsRequired = true },
            new KernelParameterMetadata("count") { Description = "Number of results", ParameterType = typeof(int), IsRequired = false, DefaultValue = 2 },
            new KernelParameterMetadata("skip") { Description = "Number of results to skip", ParameterType = typeof(int), IsRequired = false, DefaultValue = 0 },
        ];
    }

}
