using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Plugins;

/// <summary>
/// 用于提供天气信息的模拟插件。
/// </summary>
internal sealed class WeatherPlugin
{
    /// <summary>
    /// 存储天气预测信息的字典，键为日期和地点组合，值为对应的天气预测记录
    /// </summary>
    private readonly Dictionary<string, WeatherForecast> _forecasts = new();

    /// <summary>
    /// 获取当前日期。
    /// </summary>
    /// <returns>以 "yyyy-MM-dd" 格式表示的当前日期字符串。</returns>
    [KernelFunction]
    public string GetCurrentDate() => DateTime.Now.Date.ToString("yyyy-MM-dd");

    /// <summary>
    /// 提供指定日期和地点的天气预测信息。若日期超出 15 天，则使用历史数据。
    /// </summary>
    /// <param name="date">要查询的日期</param>
    /// <param name="location">要查询的地点</param>
    /// <returns>包含指定日期和地点天气预测信息的 <see cref="WeatherForecast"/> 对象。</returns>
    [KernelFunction]
    [Description("提供给定日期和地点的天气预测。超过 15 天的日期将使用历史数据。")]
    public WeatherForecast GetForecast(string date, string location)
    {
        // 生成用于查找预测信息的键
        string key = $"{date}-{location}";

        // 尝试从字典中获取预测信息
        if (!this._forecasts.TryGetValue(key, out WeatherForecast? forecast))
        {
            // 若未找到，则生成新的预测信息
            forecast = GenerateForecast(date, location);
            // 将新生成的预测信息存入字典
            this._forecasts[key] = forecast;
        }

        return forecast;
    }

    /// <summary>
    /// 生成指定日期和地点的天气预测信息。
    /// </summary>
    /// <param name="date">预测的日期</param>
    /// <param name="location">预测的地点</param>
    /// <returns>包含生成的天气预测信息的 <see cref="WeatherForecast"/> 对象。</returns>
    private static WeatherForecast GenerateForecast(string date, string location)
    {
        // 随机生成最高温度（范围 49 - 96）
        int highTemp = Random.Shared.Next(49, 96);
        // 随机生成最低温度（最高温度减去 12 - 20）
        int lowTemp = highTemp - Random.Shared.Next(12, 20);
        // 随机生成降水量（范围 0 - 80）
        int precip = Random.Shared.Next(0, 80);

        return new WeatherForecast(date, location, $"{highTemp} ℉", $"{lowTemp} ℉", $"{precip} %");
    }
}

/// <summary>
/// 表示天气预测信息的记录类型。
/// </summary>
internal sealed record WeatherForecast(
    /// <summary>
    /// 预测日期
    /// </summary>
    string Date,
    /// <summary>
    /// 预测地点
    /// </summary>
    string Location,
    /// <summary>
    /// 最高温度
    /// </summary>
    string HighTemperature,
    /// <summary>
    /// 最低温度
    /// </summary>
    string LowTemperature,
    /// <summary>
    /// 降水量
    /// </summary>
    string Precipition
);
