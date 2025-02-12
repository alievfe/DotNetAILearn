using System.Web;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace SKUtils.Web;

public class BingSearchTest : IDisposable
{
    private const string BingSearchUrl = "https://www.bing.com/search?";
    private const string BingHostUrl = "https://www.bing.com";
    private const int AbstractMaxLength = 500;

    private IPlaywright _playwright;
    private IBrowser _browser;

    public BingSearchTest()
    {
        Initialize().Wait();
    }

    private async Task Initialize()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task<List<SearchResult>> SearchAsync(
        string keyword,
        int numResults = 10,
        bool debug = false
    )
    {
        var results = new List<SearchResult>();

        if (string.IsNullOrEmpty(keyword) || numResults <= 0)
            return results;

        var page = await _browser.NewPageAsync();

        try
        {
            while (results.Count < numResults)
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "q", keyword },
                    { "FPIG", Guid.NewGuid().ToString("N") },
                    { "first", results.Count.ToString() },
                    { "FORM", "PORE" },
                };

                var searchUrl = BingSearchUrl + BuildQueryString(queryParams);
                await page.GotoAsync(searchUrl);

                var content = await page.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var resultsContainer = doc.DocumentNode.SelectSingleNode("//main[@id='b_results']");

                if (resultsContainer == null)
                {
                    Console.WriteLine("No search results found.");
                    break;
                }

                foreach (var li in resultsContainer.ChildNodes)
                {
                    if (!li.HasClass("b_algo"))
                        continue;

                    var titleNode = li.SelectSingleNode(".//h2/a");
                    var urlNode = li.SelectSingleNode(".//div[@class='b_tpcn']/a");
                    var abstractNode = li.SelectSingleNode(".//div[@class='b_caption']");

                    if (titleNode == null || urlNode == null || abstractNode == null)
                        continue;

                    var result = new SearchResult
                    {
                        Rank = results.Count + 1,
                        Title = HttpUtility.HtmlDecode(titleNode.InnerText).Trim(),
                        Url = urlNode.GetAttributeValue("href", ""),
                        Abstract = TruncateAbstract(
                            HttpUtility.HtmlDecode(abstractNode.InnerText).Trim()
                        ),
                    };

                    results.Add(result);

                    if (results.Count >= numResults)
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            if (debug)
                Console.WriteLine($"Error during search: {ex.Message}");
        }
        finally
        {
            await page.CloseAsync();
        }

        return results.Take(numResults).ToList();
    }

    private string TruncateAbstract(string text)
    {
        return text.Length <= AbstractMaxLength
            ? text
            : text.Substring(0, AbstractMaxLength) + "...";
    }

    private string BuildQueryString(Dictionary<string, string> parameters)
    {
        return string.Join(
            "&",
            parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}")
        );
    }

    public void Dispose()
    {
        _browser?.CloseAsync().Wait();
        _playwright?.Dispose();
    }
}

public class SearchResult
{
    public int Rank { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string Abstract { get; set; }
}
