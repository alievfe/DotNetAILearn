using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Plugins;

/// <summary>
/// 用于提供位置信息的模拟插件。
/// </summary>
internal sealed class LocationPlugin
{
    /// <summary>
    /// 提供用户当前所在的城市、地区和国家信息。
    /// </summary>
    /// <returns>表示用户当前位置的字符串，格式为“城市, 地区, 国家”。</returns>
    [KernelFunction]
    [Description("按城市、地区和国家提供用户的当前位置。")]
    public string GetCurrentLocation() => "贝尔维尤, 华盛顿州, 美国";

    /// <summary>
    /// 提供用户家庭所在的城市、地区和国家信息。
    /// </summary>
    /// <returns>表示用户家庭位置的字符串，格式为“城市, 地区, 国家”。</returns>
    [KernelFunction]
    [Description("按城市、地区和国家提供用户的家庭位置。")]
    public string GetHomeLocation() => "西雅图, 华盛顿州, 美国";

    /// <summary>
    /// 提供用户工作办公室所在的城市、地区和国家信息。
    /// </summary>
    /// <returns>表示用户工作办公室位置的字符串，格式为“城市, 地区, 国家”。</returns>
    [KernelFunction]
    [Description("按城市、地区和国家提供用户的工作办公室位置。")]
    public string GetOfficeLocation() => "雷德蒙德, 华盛顿州, 美国";
}
