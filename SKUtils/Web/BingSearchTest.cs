using System.Web;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace SKUtils.Web;

public class SearchResult
{
    public int Rank { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string Abstract { get; set; }
}

public class BingSearchTest : IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;

    public BingSearchTest()
    {
        _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        _browser = _playwright
            .Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false })
            .GetAwaiter()
            .GetResult();
    }

    public async Task<List<SearchResult>> SearchAsync(
        string keyword,
        int numResults = 30,
        bool debug = false
    )
    {
        var results = new List<SearchResult>();

        if (string.IsNullOrEmpty(keyword) || numResults <= 0)
            return results;

        var page = await _browser.NewPageAsync();

        while (results.Count < numResults)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "q", keyword },
                { "FPIG", Guid.NewGuid().ToString("N") },
                { "first", results.Count.ToString() },
                { "FORM", "PORE" },
            };
            string url = "https://cn.bing.com/search?" + UrlEncode(queryParams);

            try
            {
                await page.GotoAsync(
                    url,
                    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }
                );
                await page.ReloadAsync();
                var content = await page.ContentAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);

                var bResults = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='b_results']");
                if (bResults == null)
                {
                    if (debug)
                        Console.WriteLine("No search results found.");
                    break;
                }

                var resultItems = bResults.SelectNodes("./li[contains(@class, 'b_algo')]");
                if (resultItems == null || !resultItems.Any())
                {
                    if (debug)
                        Console.WriteLine("No result items on current page.");
                    break;
                }

                foreach (var li in resultItems)
                {
                    var titleNode = li.SelectSingleNode(".//h2/a");
                    var urlNode = li.SelectSingleNode(".//div[contains(@class, 'b_tpcn')]//a");
                    var abstractNode = li.SelectSingleNode(".//div[contains(@class, 'b_caption')]");

                    string title = titleNode?.InnerText.Trim();
                    string iurl = urlNode?.GetAttributeValue("href", "").Trim();
                    string abstractText = abstractNode?.InnerText.Trim();

                    if (string.IsNullOrEmpty(title))
                        continue;
                    if (string.IsNullOrEmpty(iurl))
                        continue;

                    //if (abstractText?.Length > 500)
                        //abstractText = abstractText.Substring(0, 500);

                    results.Add(
                        new SearchResult
                        {
                            Rank = results.Count + 1,
                            Title = title,
                            Url = iurl,
                            Abstract = abstractText ?? "",
                        }
                    );

                    if (results.Count >= numResults)
                        break;
                }
            }
            catch (Exception ex)
            {
                if (debug)
                    Console.WriteLine($"Error: {ex.Message}");
                break;
            }
        }

        return results.Take(numResults).ToList();
    }

    private string UrlEncode(Dictionary<string, string> parameters)
    {
        return string.Join(
            "&",
            parameters.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"
            )
        );
    }

    public void Dispose()
    {
        _browser?.CloseAsync()?.GetAwaiter().GetResult();
        _browser?.DisposeAsync().GetAwaiter().GetResult();
        _playwright?.Dispose();
    }
}
