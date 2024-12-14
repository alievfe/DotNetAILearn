using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseSKLearn.Utils;

public class WeatherAPI
{
    private readonly string _apiKey;
    public WeatherAPI(string apiKey)
    {
        _apiKey = apiKey;
    }
    // 心知天气接口
    public async Task<NowWeather> GetWeatherForCityAsync(string cityName)
    {
        const string baseUrl = "https://api.seniverse.com/v3/weather/now.json";
        using var httpClient = new HttpClient();
        var url = $"{baseUrl}?key={_apiKey}&location={cityName}&language=zh-Hans&unit=c";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Request failed with status code {response.StatusCode}"
            );
        using var jd = await JsonDocument.ParseAsync(response.Content.ReadAsStream());
        var res = jd.RootElement
            .GetProperty("results")[0]
            .GetProperty("now")
            .Deserialize<NowWeather>();
        if (res is null)
            throw new HttpRequestException($"Request failed");
        return res;
    }
}

public record NowWeather
{
    [JsonPropertyName("text")]
    public required string WeatherPhenomena { get; set; }

    [JsonPropertyName("temperature")]
    public required string Temperature { get; set; }
}
