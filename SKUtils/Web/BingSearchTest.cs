using System.Web;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace SKUtils.Web;

public class SearchResult
{
    public int Rank { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public string? SiteName { get; set; }
    public string? SiteIcon { get; set; }
    public string? Image { get; set; }
    public DateOnly? SiteDate { get; set; }
    public string? Abstract { get; set; }
}

/*
TODO: 筛选查询
 24小时

filters=ex1%3a"ez1"

一周

filters=ex1%3a"ez2"

一个月

filters=ex1%3a"ez3"

去年

filters=ex1%3a"ez5_19773_20138" 2024 2 20 2025 2 18

filters=ex1%3a"ez5_20129_20138" 202

ez5_19763_20137	2024 2 10 2025 2 18

ez5_19763_20137	2024 2 11 2025 2 18

 */

public class BingSearchTest : IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;

    private readonly string _host = "https://cn.bing.com";

    public BingSearchTest()
    {
        _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        _browser = _playwright
            .Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true })
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
                { "FORM", "PERE1" },
            };
            string url = $"{_host}/search?{UrlEncode(queryParams)}";

            try
            {
                string? content = null;
                try
                {
                    await page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = 2500
                    });
                    content = await page.ContentAsync();
                    if (content.Length < 100)
                    {
                        await page.ReloadAsync(
                            new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 2500 }
                        );
                        content = await page.ContentAsync();
                    }
                }
                catch (TimeoutException)
                {
                    content = await page.ContentAsync();
                }
                catch (Exception)
                {
                    throw;
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);

                var bResults = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='b_results']");
                if (bResults == null)
                {
                    if (debug)
                        Console.WriteLine("No search results found.");
                    break;
                }
                var totalCountNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='b_tween_searchResults']/span");
                long? totalEstimatedMatches;
                if (totalCountNode is not null)
                    totalEstimatedMatches = ParseTotalCountFromText(totalCountNode.InnerText.Trim());

                var resultItems = bResults.SelectNodes("./li[contains(@class, 'b_algo')]");
                if (resultItems == null || resultItems.Count == 0)
                {
                    if (debug)
                        Console.WriteLine("No result items on current page.");
                    break;
                }

                foreach (var li in resultItems)
                {
                    var titleNode = li.SelectSingleNode(".//h2/a");
                    var urlNode = li.SelectSingleNode(".//div[contains(@class, 'b_tpcn')]//a");
                    var iconNode = li.SelectSingleNode(".//div[contains(@class, 'wr_fav')]/div/img");
                    var abstractNode = li.SelectSingleNode(".//div[contains(@class, 'b_caption')]");
                    var abstractNode2 = li.SelectSingleNode(
                        ".//p[contains(@class, 'b_lineclamp3')]"
                    );
                    var imageNode = li.SelectSingleNode(".//a[contains(@class, 'b_ci_image_ova')]");
                    var title = titleNode?.InnerText.Trim();
                    var iurl = urlNode?.GetAttributeValue("href", "").Trim();
                    var siteName = urlNode?.GetAttributeValue("aria-label", "").Trim();
                    var iconurl = iconNode?.GetAttributeValue("src", "").Trim();

                    var abstractText = abstractNode?.InnerText.Trim();
                    if (string.IsNullOrEmpty(abstractText))
                        abstractText = abstractNode2?.InnerText.Trim() ?? "";
                    
                    string? imageurl = null;
                    if (imageNode is not null)
                        imageurl = ParseImageUrlInLable(
                            imageNode.GetAttributeValue("aria-label", "").Trim()
                        );

                    var siteDate = ParseDateInText(abstractText);

                    if (string.IsNullOrEmpty(title))
                        continue;
                    if (string.IsNullOrEmpty(iurl))
                        continue;

                    results.Add(
                        new SearchResult
                        {
                            Rank = results.Count + 1,
                            Title = title,
                            Url = iurl,
                            SiteName = siteName,
                            SiteIcon = iconurl,
                            SiteDate = siteDate,
                            Image = imageurl,
                            Abstract = abstractText,
                        }
                    );

                    if (results.Count >= numResults)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                break;
            }
        }

        return results.Take(numResults).ToList();
    }
    private long? ParseTotalCountFromText(string input)
    {
        string cleanNumber = input.Replace("约 ", "").Replace(" 个结果", "").Replace(",", "");
        // 尝试解析为 long 类型
        if (long.TryParse(cleanNumber, out long number))
            return number;
        else
            return null;
    }
    private string? ParseImageUrlInLable(string lable)
    {
        string decodedUrl = System.Net.WebUtility.HtmlDecode(lable);
        var uri = new Uri(_host + decodedUrl);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        return queryParams["mediaurl"];
    }

    private DateOnly? ParseDateInText(string input)
    {
        input = input.Length < 20 ? input : input[..20];
        int dotIndex = input.IndexOf(" · ");
        if (dotIndex == -1)
            return null;
        string datePart = input[..dotIndex].Trim();
        if (datePart.EndsWith("天前"))
        {
            int days = int.Parse(datePart.Replace("天前", "").Trim());
            return DateOnly.FromDateTime(DateTime.Today).AddDays(-days);
        }
        if (datePart.EndsWith("天之前"))
        {
            int days = int.Parse(datePart.Replace("天之前", "").Trim());
            return DateOnly.FromDateTime(DateTime.Today).AddDays(-days);
        }
        if (datePart.EndsWith("之前"))
        {
            return DateOnly.FromDateTime(DateTime.Today);
        }
        if (
            DateOnly.TryParseExact(
                datePart,
                "yyyy年M月d日",
                null,
                System.Globalization.DateTimeStyles.None,
                out var parsedDate
            )
        )
        {
            return parsedDate;
        }
        return null;
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
